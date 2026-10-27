using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TollCalculator.Application.Services;
using TollCalculator.WebApi.Host.Mappers;
using TollCalculator.WebApi.Host.Validators.Implementations;
using TollCalculator.WebApi.Models.V1.Requests;
using TollCalculator.WebApi.Models.V1.Responses;

namespace TollCalculator.WebApi.Host.Controllers.V1;

[ApiVersion("1.0")]
public sealed class TollController : ApiControllerBase
{
    private readonly ITollService _tollService;

    public TollController(ITollService tollService)
    {
        ArgumentNullException.ThrowIfNull(tollService);

        _tollService = tollService;
    }

    /// <summary>Calculates the toll for a set of passage timestamps without recording anything.</summary>
    [HttpPost("api/v{version:apiVersion}/toll/calculate")]
    [ProducesResponseType<CalculateTollResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CalculateTollResponse>> Calculate(
        [FromBody] CalculateTollRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = CalculateTollRequestValidator.Validate(request);

        if (!validationResult.IsValid)
        {
            return InvalidRequest(validationResult);
        }

        var result = await _tollService.CalculateAsync(request.VehicleType!, request.Timestamps, cancellationToken);

        if (!result.IsSuccess)
        {
            return Failure(result);
        }

        return Ok(DailyTollMapper.ToCalculateTollResponse(request.VehicleType!, result.Value!));
    }
}
