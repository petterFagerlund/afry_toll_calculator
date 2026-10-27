using Microsoft.EntityFrameworkCore;
using TollCalculator.Domain.Constants;
using TollCalculator.Domain.Models;
using TollCalculator.Domain.Repositories;
using TollCalculator.Infrastructure.Persistence;

namespace TollCalculator.Infrastructure.Repositories.Implementations;

internal sealed class TollPassageRepository : ITollPassageRepository
{
    private readonly TollDbContext _context;

    public TollPassageRepository(TollDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _context = context;
    }

    public async Task<Guid> AddAsync(TollPassage passage, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(passage);

        _context.TollPassages.Add(passage);
        await _context.SaveChangesAsync(cancellationToken);

        return passage.Id;
    }

    public async Task<TollPassage[]> GetForDayAsync(
        string registrationNumber,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        var normalized = RegistrationNumbers.Normalize(registrationNumber, nameof(registrationNumber));
        var dayStart = TollTimeZone.StartOfDay(date);
        var nextDayStart = TollTimeZone.StartOfDay(date.AddDays(1));

        return await _context.TollPassages
            .AsNoTracking()
            .Where(passage => passage.RegistrationNumber == normalized)
            .Where(passage => passage.PassedAt >= dayStart && passage.PassedAt < nextDayStart)
            .OrderBy(passage => passage.PassedAt)
            .ToArrayAsync(cancellationToken);
    }
}
