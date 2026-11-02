using TollCalculator.Domain.Models;

namespace TollCalculator.Domain.Services.Implementations;

public sealed class WeekendAndHolidayService : ITollFreeDateService
{
    private readonly IHolidayProvider _holidayProvider;

    public WeekendAndHolidayService(IHolidayProvider holidayProvider)
    {
        ArgumentNullException.ThrowIfNull(holidayProvider);

        _holidayProvider = holidayProvider;
    }

    public bool IsTollFree(DateOnly date)
    {
        if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            return true;
        }

        foreach (var holiday in _holidayProvider.GetHolidays(date.Year))
        {
            if (holiday.Date == date)
            {
                return true;
            }
        }

        return false;
    }
}
