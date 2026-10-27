using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TollCalculator.Application.Services;
using TollCalculator.WebApi.Host.Mappers;
using TollCalculator.WebApi.Host.Validators;
using TollCalculator.WebApi.Models.V1.Requests;
using TollCalculator.WebApi.Models.V1.Responses;

namespace TollCalculator.WebApi.Host.Controllers.V1;

[ApiVersion("1.0")]
public sealed class TollController : ApiControllerBase
{
    private readonly ITollService _tollService;
    private readonly IDailyTollMapper _dailyTollMapper;
    private readonly ICalculateTollRequestValidator _calculateTollRequestValidator;

    public TollController(
        ITollService tollService,
        IDailyTollMapper dailyTollMapper,
        ICalculateTollRequestValidator calculateTollRequestValidator)
    {
        ArgumentNullException.ThrowIfNull(tollService);
        ArgumentNullException.ThrowIfNull(dailyTollMapper);
        ArgumentNullException.ThrowIfNull(calculateTollRequestValidator);

        _tollService = tollService;
        _dailyTollMapper = dailyTollMapper;
        _calculateTollRequestValidator = calculateTollRequestValidator;
    }

    /// <summary>Calculates the toll for a set of passage timestamps without recording anything.</summary>
    [HttpPost("api/v{version:apiVersion}/toll/calculate")]
    [ProducesResponseType<CalculateTollResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CalculateTollResponse>> Calculate(
        [FromBody] CalculateTollRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = _calculateTollRequestValidator.Validate(request);

        if (!validationResult.IsValid)
        {
            return InvalidRequest(validationResult);
        }

        var result = await _tollService.CalculateAsync(request.VehicleType!, request.Timestamps, cancellationToken);

        if (!result.IsSuccess)
        {
            return Failure(result);
        }

        return Ok(_dailyTollMapper.ToCalculateTollResponse(request.VehicleType!, result.Value!));
    }
}
