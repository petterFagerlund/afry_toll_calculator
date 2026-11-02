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

        var holidays = _holidayProvider.GetHolidays(date.Year);

        if (!holidays.IsSuccess)
        {
            return ApplicationResult<bool>.Error(holidays.ErrorMessage!);
        }

        foreach (var holiday in holidays.Value!)
        {
            if (holiday.Date == date)
            {
                return ApplicationResult<bool>.Success(true);
            }
        }

        return ApplicationResult<bool>.Success(false);
    }
}
