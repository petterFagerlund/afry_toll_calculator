using TollCalculator.Application.Providers;
using TollCalculator.Application.Results;
using TollCalculator.Domain.Models;

namespace TollCalculator.Application.UnitTests;

internal sealed class StubHolidayProvider : IHolidayProvider
{
    private readonly Holiday[] _holidays;

    public StubHolidayProvider(params Holiday[] holidays)
    {
        _holidays = holidays;
    }

    public ApplicationResult<Holiday[]> GetHolidays(int year)
    {
        return ApplicationResult<Holiday[]>.Success(_holidays);
    }
}
