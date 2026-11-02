using TollCalculator.Application.Results;
using TollCalculator.Domain.Models;

namespace TollCalculator.Application.Services;

public interface IHolidayService
{
    ApplicationResult<Holiday[]> GetHolidays(int? year);
}
