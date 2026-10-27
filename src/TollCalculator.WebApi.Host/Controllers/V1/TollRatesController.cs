using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TollCalculator.Application.Services;
using TollCalculator.WebApi.Host.Mappers;
using TollCalculator.WebApi.Models.V1.Responses;

namespace TollCalculator.WebApi.Host.Controllers.V1;

[ApiVersion("1.0")]
public sealed class TollRatesController : ApiControllerBase
{
    private readonly ITollRateService _tollRateService;
    private readonly ITollRateMapper _tollRateMapper;

    public TollRatesController(ITollRateService tollRateService, ITollRateMapper tollRateMapper)
    {
        ArgumentNullException.ThrowIfNull(tollRateService);
        ArgumentNullException.ThrowIfNull(tollRateMapper);

        _tollRateService = tollRateService;
        _tollRateMapper = tollRateMapper;
    }

    /// <summary>Returns the toll rate intervals that make up the daily schedule.</summary>
    [HttpGet("api/v{version:apiVersion}/toll-rates")]
    [ProducesResponseType<TollRateResponse[]>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TollRateResponse[]>> GetTollRates(CancellationToken cancellationToken)
    {
        var result = await _tollRateService.GetTollRatesAsync(cancellationToken);

        if (!result.IsSuccess)
        {
            return Failure(result);
        }

        return Ok(_tollRateMapper.ToResponses(result.Value!));
    }
}
