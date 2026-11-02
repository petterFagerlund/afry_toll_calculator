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
        TollPassage[] tollPassages,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tollPassages);

        var recordTollPassages = await _tollPassageRepository.AddRangeAsync(tollPassages, cancellationToken);
        if (!recordTollPassages.IsSuccess)
        {
            _logger.LogError(
                "A batch of {PassageCount} toll passages was not recorded: {Reason}",
                tollPassages.Length,
                recordTollPassages.ErrorMessage);

            return recordTollPassages;
        }

        if (recordTollPassages.Value!.Length != tollPassages.Length)
        {
            _logger.LogError(
                "Recorded {RecordedCount} toll passages but {RequestedCount} were requested.",
                recordTollPassages.Value!.Length,
                tollPassages.Length);

            return ApplicationResult<Guid[]>.Error("The recorded toll passages do not match the requested ones.");
        }

        return recordTollPassages;
    }
}
