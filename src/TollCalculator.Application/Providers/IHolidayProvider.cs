using TollCalculator.Application.Results;
using TollCalculator.Domain.Models;

namespace TollCalculator.Application.Providers;

public interface IHolidayProvider
{
    ApplicationResult<Holiday[]> GetHolidays(int year);
}
