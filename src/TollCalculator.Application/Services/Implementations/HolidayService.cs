using TollCalculator.Application.Results;
using TollCalculator.Domain.Models;
using TollCalculator.Domain.Repositories;

namespace TollCalculator.Application.Services.Implementations;

internal sealed class HolidayService : IHolidayService
{
    private const int EarliestYear = 1;
    private const int LatestYear = 9999;

    private readonly IHolidayRepository _holidayRepository;

    public HolidayService(IHolidayRepository holidayRepository)
    {
        ArgumentNullException.ThrowIfNull(holidayRepository);

        _holidayRepository = holidayRepository;
    }

    public async Task<ApplicationResult<Holiday[]>> GetHolidaysAsync(int? year, CancellationToken cancellationToken)
    {
        if (year is < EarliestYear or > LatestYear)
        {
            return ApplicationResult<Holiday[]>.Invalid(
                $"The year {year} is outside the supported range {EarliestYear}-{LatestYear}.");
        }

        if (year is null)
        {
            return ApplicationResult<Holiday[]>.Success(await _holidayRepository.GetAllAsync(cancellationToken));
        }

        var holidays = await _holidayRepository.GetByYearAsync(year.Value, cancellationToken);

        return ApplicationResult<Holiday[]>.Success(holidays);
    }
}
