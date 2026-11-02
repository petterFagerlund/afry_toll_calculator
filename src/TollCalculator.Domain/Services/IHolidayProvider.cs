using TollCalculator.Domain.Models;

namespace TollCalculator.Domain.Services;

public interface IHolidayProvider
{
    Holiday[] GetHolidays(int year);
}
