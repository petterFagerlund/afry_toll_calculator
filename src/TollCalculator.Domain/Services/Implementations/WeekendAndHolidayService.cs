using TollCalculator.Domain.Models;

namespace TollCalculator.Domain.Services.Implementations;

public sealed class WeekendAndHolidayService : ITollFreeDateService
{
    private readonly HashSet<DateOnly> _holidays;

    public WeekendAndHolidayService(Holiday[] holidays)
    {
        ArgumentNullException.ThrowIfNull(holidays);

        _holidays = [];

        foreach (var holiday in holidays)
        {
            _holidays.Add(holiday.Date);
        }
    }

    public bool IsTollFree(DateOnly date)
    {
        if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            return true;
        }

        return _holidays.Contains(date);
    }
}
