using TollCalculator.Application.Results;
using TollCalculator.Domain.Models;

namespace TollCalculator.Application.Repositories;

public interface ITollPassageRepository
{
    Task<ApplicationResult<Guid>> AddAsync(TollPassage passage, CancellationToken cancellationToken);

    Task<TollPassage[]> GetForDayAsync(string registrationNumber, DateOnly date, CancellationToken cancellationToken);
}
