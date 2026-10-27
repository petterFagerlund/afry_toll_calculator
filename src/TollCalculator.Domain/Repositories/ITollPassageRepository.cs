using TollCalculator.Domain.Models;

namespace TollCalculator.Domain.Repositories;

public interface ITollPassageRepository
{
    Task<Guid> AddAsync(TollPassage passage, CancellationToken cancellationToken);

    Task<TollPassage[]> GetForDayAsync(string registrationNumber, DateOnly date, CancellationToken cancellationToken);
}
