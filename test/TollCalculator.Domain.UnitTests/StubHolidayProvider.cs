using TollCalculator.Domain.Models;
using TollCalculator.Domain.Services;

namespace TollCalculator.Domain.UnitTests;

internal sealed class StubHolidayProvider : IHolidayProvider
{
    private readonly Holiday[] _holidays;

    public StubHolidayProvider(params Holiday[] holidays)
    {
        _holidays = holidays;
    }

    public Holiday[] GetHolidays(int year)
    {
        return _holidays;
    }
}
