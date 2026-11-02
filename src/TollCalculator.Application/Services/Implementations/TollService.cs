using TollCalculator.Application.Results;
using TollCalculator.Domain.Enums;
using TollCalculator.Domain.Models;
using TollCalculator.Domain.Repositories;
using TollCalculator.Domain.Services;

namespace TollCalculator.Application.Services.Implementations;

internal sealed class TollService : ITollService
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly ITollPassageRepository _tollPassageRepository;
    private readonly ITollScheduleRepository _tollScheduleRepository;
    private readonly IDailyTollService _dailyTollService;

    public TollService(
        IVehicleRepository vehicleRepository,
        ITollPassageRepository tollPassageRepository,
        ITollScheduleRepository tollScheduleRepository,
        IDailyTollService dailyTollService)
    {
        ArgumentNullException.ThrowIfNull(vehicleRepository);
        ArgumentNullException.ThrowIfNull(tollPassageRepository);
        ArgumentNullException.ThrowIfNull(tollScheduleRepository);
        ArgumentNullException.ThrowIfNull(dailyTollService);

        _vehicleRepository = vehicleRepository;
        _tollPassageRepository = tollPassageRepository;
        _tollScheduleRepository = tollScheduleRepository;
        _dailyTollService = dailyTollService;
    }

    public async Task<ApplicationResult<DailyToll>> GetDailyTollAsync(
        string registrationNumber,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleRepository.GetAsync(registrationNumber, cancellationToken);

        if (vehicle is null)
        {
            return ApplicationResult<DailyToll>.NotFound(
                $"No vehicle is registered with the number '{registrationNumber.Trim().ToUpperInvariant()}'.");
        }

        var passages = await _tollPassageRepository.GetForDayAsync(registrationNumber, date, cancellationToken);
        var timestamps = new List<DateTimeOffset>();

        foreach (var passage in passages)
        {
            timestamps.Add(passage.PassedAt);
        }

        return await CalculateTollAsync(vehicle.VehicleType, timestamps.ToArray(), cancellationToken);
    }

    public async Task<ApplicationResult<DailyToll>> CalculateAsync(
        VehicleType vehicleType,
        DateTimeOffset[] timestamps,
        CancellationToken cancellationToken)
    {
        return await CalculateTollAsync(vehicleType, timestamps, cancellationToken);
    }

    private async Task<ApplicationResult<DailyToll>> CalculateTollAsync(
        VehicleType vehicleType,
        DateTimeOffset[] passedAt,
        CancellationToken cancellationToken)
    {
        var tariff = await _tollScheduleRepository.GetTariffAsync(cancellationToken);
        return ApplicationResult<DailyToll>.Success(_dailyTollService.Calculate(vehicleType, passedAt, tariff));
    }
}
