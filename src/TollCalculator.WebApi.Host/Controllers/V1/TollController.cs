using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TollCalculator.Application.Services;
using TollCalculator.Domain.Enums;
using TollCalculator.Domain.Models;
using TollCalculator.WebApi.Host.Mappers;
using TollCalculator.WebApi.Host.Validators.Implementations;
using TollCalculator.WebApi.Models.V1.Requests;
using TollCalculator.WebApi.Models.V1.Responses;

namespace TollCalculator.WebApi.Host.Controllers.V1;

[ApiVersion("1.0")]
public sealed class TollController : ApiControllerBase
{
    private readonly ITollPassageService _tollPassageService;
    private readonly ITollService _tollService;
    private readonly TimeProvider _timeProvider;

    public TollController(
        ITollPassageService tollPassageService,
        ITollService tollService,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(tollPassageService);
        ArgumentNullException.ThrowIfNull(tollService);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _tollPassageService = tollPassageService;
        _tollService = tollService;
        _timeProvider = timeProvider;
    }

    /// <summary>Records that one or more vehicles passed a toll station.</summary>
    /// <remarks>
    /// A passage is an observation, so any registration number is accepted whether or not it is registered as
    /// a vehicle. Each passage carries the vehicle type the roadside equipment reported, and that type is what
    /// the daily toll is priced from. The batch is recorded in one transaction: if any passage repeats one that
    /// is already recorded, the whole batch is rejected with 409 and nothing is written.
    /// </remarks>
    [HttpPost("api/v{version:apiVersion}/toll-passages")]
    [ProducesResponseType<RecordTollPassagesResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RecordTollPassagesResponse>> RecordTollPassages(
        [FromBody] RecordTollPassagesRequest request,
        CancellationToken cancellationToken)
    {
        var modelState = RecordTollPassagesRequestValidator.Validate(request, _timeProvider.GetUtcNow());
        if (!modelState.IsValid)
        {
            return MapValidationProblem(modelState);
        }

        var passages = Array.ConvertAll(request.Passages, ToTollPassage);
        var recordedTollPassages = await _tollPassageService.RecordTollPassagesAsync(passages, cancellationToken);
        if (!recordedTollPassages.IsSuccess)
        {
            return MapErrorResponse(recordedTollPassages);
        }

        var response = new RecordTollPassagesResponse { TollPassageIds = recordedTollPassages.Value! };
        return StatusCode(StatusCodes.Status201Created, response);
    }

    /// <summary>Returns the toll a vehicle owes for a single day, with the passages that were charged.</summary>
    /// <remarks>
    /// A registration number with no passages that day owes nothing, so the answer is 200 with a zero fee
    /// rather than 404. Only charged passages are listed: a passage sharing its 60-minute window with a dearer
    /// one, and every passage after the daily cap is reached, costs nothing and is left out of the list.
    /// </remarks>
    [HttpPost("api/v{version:apiVersion}/toll-calculations")]
    [ProducesResponseType<DailyTollResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DailyTollResponse>> CalculateDailyToll(
        [FromBody] CalculateDailyTollRequest request,
        CancellationToken cancellationToken)
    {
        var modelState = CalculateDailyTollRequestValidator.Validate(request, _timeProvider.GetUtcNow());
        if (!modelState.IsValid)
        {
            return MapValidationProblem(modelState);
        }

        var result = await _tollService.GetDailyTollAsync(
            request.RegistrationNumber, request.Date, cancellationToken);
        if (!result.IsSuccess)
        {
            return MapErrorResponse(result);
        }

        var normalizedRegNr = RegistrationNumbers.Normalize(request.RegistrationNumber);
        return Ok(DailyTollMapper.ToDailyTollResponse(normalizedRegNr, request.Date, result.Value!));
    }

    private static TollPassage ToTollPassage(TollPassageToRecord passage)
    {
        VehicleTypeParser.TryParse(passage.VehicleType, out var vehicleType);
        return TollPassage.Create(passage.RegistrationNumber, vehicleType, passage.PassedAt);
    }
}
