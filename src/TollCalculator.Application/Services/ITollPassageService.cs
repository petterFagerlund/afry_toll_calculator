using TollCalculator.Application.Results;

namespace TollCalculator.Application.Services;

public interface ITollPassageService
{
    Task<ApplicationResult<Guid>> RecordTollPassageAsync(
        string registrationNumber,
        DateTimeOffset passedAt,
        CancellationToken cancellationToken);
}
