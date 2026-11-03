using TollCalculator.Application.Providers;
using TollCalculator.Application.Results;

namespace TollCalculator.Application.Services.Implementations;

internal sealed class TollFreeDateService : ITollFreeDateService
{
    private readonly IHolidayProvider _holidayProvider;

    public TollFreeDateService(IHolidayProvider holidayProvider)
    {
        ArgumentNullException.ThrowIfNull(holidayProvider);

        _holidayProvider = holidayProvider;
    }

    public ApplicationResult<bool> IsTollFree(DateOnly date)
    {
        if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            return ApplicationResult<bool>.Success(true);
        }

        return _holidayProvider.GetHolidays(date.Year).Map(holidays => holidays.Any(holiday => holiday.Date == date));
    }
}
