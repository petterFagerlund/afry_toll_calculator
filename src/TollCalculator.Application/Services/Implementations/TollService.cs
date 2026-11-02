using TollCalculator.Application.Repositories;
using TollCalculator.Application.Results;
using TollCalculator.Domain.Models;

namespace TollCalculator.Application.Services.Implementations;

internal sealed class TollService : ITollService
{
    private readonly ITollPassageRepository _tollPassageRepository;
    private readonly ITollScheduleRepository _tollScheduleRepository;
    private readonly IDailyTollService _dailyTollService;

    public TollService(
        ITollPassageRepository tollPassageRepository,
        ITollScheduleRepository tollScheduleRepository,
        IDailyTollService dailyTollService)
    {
        ArgumentNullException.ThrowIfNull(tollPassageRepository);
        ArgumentNullException.ThrowIfNull(tollScheduleRepository);
        ArgumentNullException.ThrowIfNull(dailyTollService);

        _tollPassageRepository = tollPassageRepository;
        _tollScheduleRepository = tollScheduleRepository;
        _dailyTollService = dailyTollService;
    }

    public async Task<ApplicationResult<DailyToll>> GetDailyTollAsync(
        string registrationNumber,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        var passages = await _tollPassageRepository.GetForDayAsync(registrationNumber, date, cancellationToken);
        if (passages.Length == 0)
        {
            return ApplicationResult<DailyToll>.Success(DailyToll.None());
        }

        var tariff = await _tollScheduleRepository.GetTariffAsync(cancellationToken);

        return _dailyTollService.Calculate(passages, tariff);
    }
}
