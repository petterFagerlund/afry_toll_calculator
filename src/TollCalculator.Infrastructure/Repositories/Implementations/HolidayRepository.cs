using Microsoft.EntityFrameworkCore;
using TollCalculator.Domain.Models;
using TollCalculator.Domain.Repositories;
using TollCalculator.Infrastructure.Persistence;

namespace TollCalculator.Infrastructure.Repositories.Implementations;

internal sealed class HolidayRepository : IHolidayRepository
{
    private readonly TollDbContext _context;

    public HolidayRepository(TollDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _context = context;
    }

    public async Task<Holiday[]> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.Holidays
            .AsNoTracking()
            .OrderBy(holiday => holiday.Date)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<Holiday[]> GetByYearAsync(int year, CancellationToken cancellationToken)
    {
        var firstDay = new DateOnly(year, 1, 1);
        var firstDayOfNextYear = new DateOnly(year + 1, 1, 1);

        return await _context.Holidays
            .AsNoTracking()
            .Where(holiday => holiday.Date >= firstDay && holiday.Date < firstDayOfNextYear)
            .OrderBy(holiday => holiday.Date)
            .ToArrayAsync(cancellationToken);
    }
}
