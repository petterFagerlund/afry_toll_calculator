using TollCalculator.Application.Results;
using TollCalculator.Domain.Models;
using TollCalculator.Domain.Services;

namespace TollCalculator.Application.Services.Implementations;

internal sealed class HolidayService : IHolidayService
{
    private const int EarliestYear = 1;
    private const int LatestYear = 9999;

    private readonly IHolidayProvider _holidayProvider;

    public HolidayService(IHolidayProvider holidayProvider)
    {
        ArgumentNullException.ThrowIfNull(holidayProvider);

        _holidayProvider = holidayProvider;
    }

    public ApplicationResult<Holiday[]> GetHolidays(int? year)
    {
        if (year is < EarliestYear or > LatestYear)
        {
            return ApplicationResult<Holiday[]>.Invalid(
                $"The year {year} is outside the supported range {EarliestYear}-{LatestYear}.");
        }

        return ApplicationResult<Holiday[]>.Success(_holidayProvider.GetHolidays(year ?? DateTime.UtcNow.Year));
    }
}
