using TollCalculator.Application.Results;
using TollCalculator.Domain.Models;

namespace TollCalculator.Application.Repositories;

public interface ITollPassageRepository
{
    Task<ApplicationResult<Guid[]>> AddRangeAsync(TollPassage[] passages, CancellationToken cancellationToken);

    Task<TollPassage[]> GetForDayAsync(string registrationNumber, DateOnly date, CancellationToken cancellationToken);
}
