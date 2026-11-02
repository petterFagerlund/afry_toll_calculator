using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TollCalculator.Application.Repositories;
using TollCalculator.Application.Results;
using TollCalculator.Domain.Constants;
using TollCalculator.Domain.Models;
using TollCalculator.Infrastructure.Persistence;

namespace TollCalculator.Infrastructure.Repositories.Implementations;

internal sealed class TollPassageRepository : ITollPassageRepository
{
    private const int UniqueIndexViolation = 2601;
    private const int UniqueConstraintViolation = 2627;
    private readonly TollDbContext _context;

    public TollPassageRepository(TollDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _context = context;
    }

    public async Task<ApplicationResult<Guid>> AddAsync(TollPassage passage, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(passage);

        _context.TollPassages.Add(passage);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsDuplicatePassage(exception))
        {
            _context.Entry(passage).State = EntityState.Detached;

            return ApplicationResult<Guid>.Conflict(
                $"A passage for '{passage.RegistrationNumber}' at {passage.PassedAt:O} is already recorded.");
        }

        return ApplicationResult<Guid>.Success(passage.Id);
    }

    public async Task<TollPassage[]> GetForDayAsync(
        string registrationNumber,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        var normalized = RegistrationNumbers.Normalize(registrationNumber);
        var dayStart = TollTimeZone.StartOfDay(date);
        var nextDayStart = TollTimeZone.StartOfDay(date.AddDays(1));

        return await _context.TollPassages
            .AsNoTracking()
            .Where(passage => passage.RegistrationNumber == normalized)
            .Where(passage => passage.PassedAt >= dayStart && passage.PassedAt < nextDayStart)
            .OrderBy(passage => passage.PassedAt)
            .ToArrayAsync(cancellationToken);
    }

    private static bool IsDuplicatePassage(DbUpdateException exception)
    {
        if (exception.InnerException is not SqlException sqlException)
        {
            return false;
        }

        return sqlException.Number is UniqueIndexViolation or UniqueConstraintViolation;
    }
}