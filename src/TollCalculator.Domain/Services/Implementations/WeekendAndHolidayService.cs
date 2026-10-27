using TollCalculator.Domain.Models;

namespace TollCalculator.Domain.Services.Implementations;

public sealed class WeekendAndHolidayService : ITollFreeDateService
{
    public bool IsTollFree(DateOnly date, Holiday[] holidays)
    {
        ArgumentNullException.ThrowIfNull(holidays);

        if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            return true;
        }

        foreach (var holiday in holidays)
        {
            if (holiday.Date == date)
            {
                return true;
            }
        }

        return false;
    }
}
