using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TollCalculator.Application.Services;
using TollCalculator.Domain.Constants;
using TollCalculator.WebApi.Host.Mappers;
using TollCalculator.WebApi.Host.Validators.Implementations;
using TollCalculator.WebApi.Models.V1.Requests;
using TollCalculator.WebApi.Models.V1.Responses;

namespace TollCalculator.WebApi.Host.Controllers.V1;

[ApiVersion("1.0")]
public sealed class TollPassagesController : ApiControllerBase
{
    private readonly ITollPassageService _tollPassageService;
    private readonly ITollService _tollService;

    public TollPassagesController(ITollPassageService tollPassageService, ITollService tollService)
    {
        ArgumentNullException.ThrowIfNull(tollPassageService);
        ArgumentNullException.ThrowIfNull(tollService);

        _tollPassageService = tollPassageService;
        _tollService = tollService;
    }

    /// <summary>Records that a vehicle passed a toll station.</summary>
    [HttpPost("api/v{version:apiVersion}/toll-passages")]
    [ProducesResponseType<RecordTollPassageResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecordTollPassageResponse>> RecordTollPassage(
        [FromBody] RecordTollPassageRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = RecordTollPassageRequestValidator.Validate(request);

        if (!validationResult.IsValid)
        {
            return InvalidRequest(validationResult);
        }

        var result = await _tollPassageService.RecordTollPassageAsync(
            request.RegistrationNumber!, request.PassedAt, cancellationToken);

        if (!result.IsSuccess)
        {
            return Failure(result);
        }

        var routeValues = new
        {
            version = "1.0",
            registrationNumber = request.RegistrationNumber!,
            date = TollTimeZone.DateOf(request.PassedAt),
        };

        var response = new RecordTollPassageResponse { TollPassageId = result.Value };

        return CreatedAtAction(nameof(GetDailyToll), routeValues, response);
    }

    /// <summary>Returns the toll a vehicle owes for a single day, with the passages that were charged.</summary>
    [HttpGet("api/v{version:apiVersion}/vehicles/{registrationNumber}/toll")]
    [ProducesResponseType<DailyTollResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DailyTollResponse>> GetDailyToll(
        string registrationNumber,
        [FromQuery] DateOnly? date,
        CancellationToken cancellationToken)
    {
        var validationResult = DailyTollQueryValidator.Validate(registrationNumber, date);

        if (!validationResult.IsValid)
        {
            return InvalidRequest(validationResult);
        }

        var result = await _tollService.GetDailyTollAsync(registrationNumber, date!.Value, cancellationToken);

        if (!result.IsSuccess)
        {
            return Failure(result);
        }

        return Ok(DailyTollMapper.ToDailyTollResponse(registrationNumber, date.Value, result.Value!));
    }
}
