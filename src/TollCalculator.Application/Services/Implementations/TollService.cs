using TollCalculator.Application.Repositories;
using TollCalculator.Application.Results;
using TollCalculator.Domain.Models;

namespace TollCalculator.Application.Services.Implementations;

internal sealed class TollService : ITollService
{
    private readonly ITollPassageRepository _tollPassageRepository;
    private readonly IDailyTollService _dailyTollService;

    public TollService(
        ITollPassageRepository tollPassageRepository,
        IDailyTollService dailyTollService)
    {
        ArgumentNullException.ThrowIfNull(tollPassageRepository);
        ArgumentNullException.ThrowIfNull(dailyTollService);

        _tollPassageRepository = tollPassageRepository;
        _dailyTollService = dailyTollService;
    }

    public async Task<ApplicationResult<DailyToll>> GetDailyTollAsync(
        string registrationNumber,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        var tollPassages = await _tollPassageRepository.GetForDayAsync(registrationNumber, date, cancellationToken);
        if (tollPassages.Length == 0)
        {
            return ApplicationResult<DailyToll>.Success(DailyToll.None());
        }

        return await _dailyTollService.Calculate(tollPassages, cancellationToken);
    }
}
