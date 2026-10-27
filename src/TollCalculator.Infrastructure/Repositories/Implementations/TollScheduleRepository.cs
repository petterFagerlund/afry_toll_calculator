using Microsoft.EntityFrameworkCore;
using TollCalculator.Domain.Models;
using TollCalculator.Domain.Repositories;
using TollCalculator.Infrastructure.Persistence;

namespace TollCalculator.Infrastructure.Repositories.Implementations;

internal sealed class TollScheduleRepository : ITollScheduleRepository
{
    private readonly TollDbContext _context;

    public TollScheduleRepository(TollDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _context = context;
    }

    public async Task<TollTariff> GetTariffAsync(CancellationToken cancellationToken)
    {
        var intervals = await GetRateIntervalsAsync(cancellationToken);

        var settings = await _context.TollSettings
            .AsNoTracking()
            .SingleAsync(cancellationToken);

        return TollTariff.Create(TollSchedule.Create(intervals), settings.DailyCap);
    }

    public async Task<TollRateInterval[]> GetRateIntervalsAsync(CancellationToken cancellationToken)
    {
        return await _context.TollRateIntervals
            .AsNoTracking()
            .OrderBy(interval => interval.Start)
            .ToArrayAsync(cancellationToken);
    }
}
