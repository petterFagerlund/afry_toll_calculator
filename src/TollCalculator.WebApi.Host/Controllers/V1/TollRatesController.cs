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

    public TollRatesController(ITollRateService tollRateService)
    {
        ArgumentNullException.ThrowIfNull(tollRateService);

        _tollRateService = tollRateService;
    }

    /// <summary>Returns the toll rate intervals that make up the daily schedule.</summary>
    [HttpGet("api/v{version:apiVersion}/toll-rates")]
    [ProducesResponseType<TollRateResponse[]>(StatusCodes.Status200OK)]
    public async Task<ActionResult<TollRateResponse[]>> GetTollRates(CancellationToken cancellationToken)
    {
        var intervals = await _tollRateService.GetTollRatesAsync(cancellationToken);

        return Ok(TollRateMapper.ToResponses(intervals));
    }
}
