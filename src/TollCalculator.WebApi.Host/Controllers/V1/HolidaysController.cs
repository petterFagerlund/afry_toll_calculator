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

    /// <summary>Returns the toll-free holidays for a year, defaulting to the current one.</summary>
    [HttpGet("api/v{version:apiVersion}/holidays")]
    [ProducesResponseType<HolidayResponse[]>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public ActionResult<HolidayResponse[]> GetHolidays([FromQuery] int? year)
    {
        var result = _holidayService.GetHolidays(year);

        if (!result.IsSuccess)
        {
            return MapErrorResponse(result);
        }

        return Ok(HolidayMapper.ToResponses(result.Value!));
    }
}
