using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TollCalculator.Application.Repositories;
using TollCalculator.Domain.Models;
using TollCalculator.Infrastructure.Configuration;
using TollCalculator.Infrastructure.Persistence;

namespace TollCalculator.Infrastructure.Repositories.Implementations;

internal sealed class TollScheduleRepository : ITollScheduleRepository
{
    private readonly TollDbContext _context;
    private readonly TollOptions _tollOptions;

    public TollScheduleRepository(TollDbContext context, IOptions<TollOptions> tollOptions)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(tollOptions);

        _context = context;
        _tollOptions = tollOptions.Value;
    }

    public async Task<TollTariff> GetTariffAsync(CancellationToken cancellationToken)
    {
        var intervals = await GetRateIntervalsAsync(cancellationToken);
        return TollTariff.Create(TollSchedule.Create(intervals), _tollOptions.DailyCap);
    }

    private async Task<TollRateInterval[]> GetRateIntervalsAsync(CancellationToken cancellationToken)
    {
        return await _context.TollRateIntervals
            .AsNoTracking()
            .OrderBy(interval => interval.Start)
            .ToArrayAsync(cancellationToken);
    }
}
