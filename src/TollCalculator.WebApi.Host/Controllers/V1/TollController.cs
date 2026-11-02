using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TollCalculator.Application.Services;
using TollCalculator.Domain.Constants;
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

    /// <summary>Records that a vehicle passed a toll station.</summary>
    /// <remarks>
    /// The same vehicle cannot be recorded twice at the same instant; a repeated passage answers 409.
    /// </remarks>
    [HttpPost("api/v{version:apiVersion}/toll-passages")]
    [ProducesResponseType<RecordTollPassageResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RecordTollPassageResponse>> RecordTollPassage(
        [FromBody] RecordTollPassageRequest request,
        CancellationToken cancellationToken)
    {
        var modelState = RecordTollPassageRequestValidator.Validate(request, _timeProvider.GetUtcNow());

        if (!modelState.IsValid)
        {
            return MapValidationProblem(modelState);
        }

        var result = await _tollPassageService.RecordTollPassageAsync(
            request.RegistrationNumber!, request.PassedAt, cancellationToken);

        if (!result.IsSuccess)
        {
            return MapErrorResponse(result);
        }

        var routeValues = new
        {
            version = "1.0",
            registrationNumber = RegistrationNumbers.Normalize(request.RegistrationNumber!),
            date = TollTimeZone.DateOf(request.PassedAt),
        };

        var response = new RecordTollPassageResponse { TollPassageId = result.Value };

        return CreatedAtAction(nameof(GetDailyToll), routeValues, response);
    }

    /// <summary>Returns the toll a vehicle owes for a single day, with the passages that were charged.</summary>
    /// <remarks>
    /// Only charged passages are listed. A passage that shares its 60-minute window with a dearer one, and
    /// every passage after the daily cap is reached, costs nothing and is therefore left out of the list.
    /// </remarks>
    [HttpGet("api/v{version:apiVersion}/vehicles/{registrationNumber}/toll")]
    [ProducesResponseType<DailyTollResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DailyTollResponse>> GetDailyToll(
        string registrationNumber,
        [FromQuery] DateOnly? date,
        CancellationToken cancellationToken)
    {
        var modelState = DailyTollQueryValidator.Validate(registrationNumber, date);
        if (!modelState.IsValid)
        {
            return MapValidationProblem(modelState);
        }

        var result = await _tollService.GetDailyTollAsync(registrationNumber, date!.Value, cancellationToken);
        if (!result.IsSuccess)
        {
            return MapErrorResponse(result);
        }

        var normalized = RegistrationNumbers.Normalize(registrationNumber);

        return Ok(DailyTollMapper.ToDailyTollResponse(normalized, date.Value, result.Value!));
    }
}
