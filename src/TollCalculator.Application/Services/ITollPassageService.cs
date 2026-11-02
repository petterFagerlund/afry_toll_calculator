using TollCalculator.Application.Results;
using TollCalculator.Domain.Models;

namespace TollCalculator.Application.Services;

public interface ITollPassageService
{
    Task<ApplicationResult<Guid[]>> RecordTollPassagesAsync(
        TollPassage[] passages,
        CancellationToken cancellationToken);
}
