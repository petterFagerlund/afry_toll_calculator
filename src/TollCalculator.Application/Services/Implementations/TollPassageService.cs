using Microsoft.Extensions.Logging;
using TollCalculator.Application.Repositories;
using TollCalculator.Application.Results;
using TollCalculator.Domain.Models;

namespace TollCalculator.Application.Services.Implementations;

internal sealed class TollPassageService : ITollPassageService
{
    private readonly ITollPassageRepository _tollPassageRepository;
    private readonly ILogger<TollPassageService> _logger;

    public TollPassageService(
        ITollPassageRepository tollPassageRepository,
        ILogger<TollPassageService> logger)
    {
        ArgumentNullException.ThrowIfNull(tollPassageRepository);
        ArgumentNullException.ThrowIfNull(logger);

        _tollPassageRepository = tollPassageRepository;
        _logger = logger;
    }

    public async Task<ApplicationResult<Guid[]>> RecordTollPassagesAsync(
        TollPassage[] passages,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(passages);

        var recorded = await _tollPassageRepository.AddRangeAsync(passages, cancellationToken);

        if (!recorded.IsSuccess)
        {
            _logger.LogInformation(
                "A batch of {PassageCount} passages was not recorded: {Reason}",
                passages.Length,
                recorded.ErrorMessage);

            return recorded;
        }

        _logger.LogInformation("Recorded {PassageCount} toll passages.", passages.Length);

        return recorded;
    }
}
