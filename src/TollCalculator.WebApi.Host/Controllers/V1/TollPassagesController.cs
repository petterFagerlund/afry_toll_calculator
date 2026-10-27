using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TollCalculator.Application.Services;
using TollCalculator.Domain.Constants;
using TollCalculator.WebApi.Host.Mappers;
using TollCalculator.WebApi.Host.Validators;
using TollCalculator.WebApi.Models.V1.Requests;
using TollCalculator.WebApi.Models.V1.Responses;

namespace TollCalculator.WebApi.Host.Controllers.V1;

[ApiVersion("1.0")]
public sealed class TollPassagesController : ApiControllerBase
{
    private readonly ITollPassageService _tollPassageService;
    private readonly ITollService _tollService;
    private readonly ITollPassageMapper _tollPassageMapper;
    private readonly IDailyTollMapper _dailyTollMapper;
    private readonly IRecordTollPassageRequestValidator _recordTollPassageRequestValidator;
    private readonly IDailyTollQueryValidator _dailyTollQueryValidator;

    public TollPassagesController(
        ITollPassageService tollPassageService,
        ITollService tollService,
        ITollPassageMapper tollPassageMapper,
        IDailyTollMapper dailyTollMapper,
        IRecordTollPassageRequestValidator recordTollPassageRequestValidator,
        IDailyTollQueryValidator dailyTollQueryValidator)
    {
        ArgumentNullException.ThrowIfNull(tollPassageService);
        ArgumentNullException.ThrowIfNull(tollService);
        ArgumentNullException.ThrowIfNull(tollPassageMapper);
        ArgumentNullException.ThrowIfNull(dailyTollMapper);
        ArgumentNullException.ThrowIfNull(recordTollPassageRequestValidator);
        ArgumentNullException.ThrowIfNull(dailyTollQueryValidator);

        _tollPassageService = tollPassageService;
        _tollService = tollService;
        _tollPassageMapper = tollPassageMapper;
        _dailyTollMapper = dailyTollMapper;
        _recordTollPassageRequestValidator = recordTollPassageRequestValidator;
        _dailyTollQueryValidator = dailyTollQueryValidator;
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
        var validationResult = _recordTollPassageRequestValidator.Validate(request);

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

        return CreatedAtAction(nameof(GetDailyToll), routeValues, _tollPassageMapper.ToResponse(result.Value));
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
        var validationResult = _dailyTollQueryValidator.Validate(registrationNumber, date);

        if (!validationResult.IsValid)
        {
            return InvalidRequest(validationResult);
        }

        var result = await _tollService.GetDailyTollAsync(registrationNumber, date!.Value, cancellationToken);

        if (!result.IsSuccess)
        {
            return Failure(result);
        }

        return Ok(_dailyTollMapper.ToDailyTollResponse(registrationNumber, date.Value, result.Value!));
    }
}
