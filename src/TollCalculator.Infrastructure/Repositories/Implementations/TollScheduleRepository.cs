using Microsoft.EntityFrameworkCore;
using TollCalculator.Domain.Models;
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

    public async Task<TollSchedule> GetScheduleAsync(CancellationToken cancellationToken)
    {
        var intervals = await _context.TollRateIntervals
            .AsNoTracking()
            .OrderBy(interval => interval.Start)
            .ToArrayAsync(cancellationToken);

        return TollSchedule.Create(intervals);
    }

    public async Task<int> GetDailyCapAsync(CancellationToken cancellationToken)
    {
        var settings = await _context.TollSettings
            .AsNoTracking()
            .SingleAsync(cancellationToken);

        return settings.DailyCap;
    }
}
