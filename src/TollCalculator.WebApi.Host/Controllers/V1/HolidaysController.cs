using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TollCalculator.Application.Services;
using TollCalculator.WebApi.Host.Mappers;
using TollCalculator.WebApi.Models.V1.Responses;

namespace TollCalculator.WebApi.Host.Controllers.V1;

[ApiVersion("1.0")]
public sealed class HolidaysController : ApiControllerBase
{
    private readonly IHolidayService _holidayService;

    public HolidaysController(IHolidayService holidayService)
    {
        ArgumentNullException.ThrowIfNull(holidayService);

        _holidayService = holidayService;
    }

    /// <summary>Returns the toll-free holidays, optionally narrowed to a single year.</summary>
    [HttpGet("api/v{version:apiVersion}/holidays")]
    [ProducesResponseType<HolidayResponse[]>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<HolidayResponse[]>> GetHolidays(
        [FromQuery] int? year,
        CancellationToken cancellationToken)
    {
        var result = await _holidayService.GetHolidaysAsync(year, cancellationToken);

        if (!result.IsSuccess)
        {
            return Failure(result);
        }

        return Ok(HolidayMapper.ToResponses(result.Value!));
    }
}
