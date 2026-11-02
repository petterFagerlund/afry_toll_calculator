using Microsoft.Extensions.Logging;
using TollCalculator.Application.Results;
using TollCalculator.Domain.Enums;
using TollCalculator.Domain.Models;
using TollCalculator.Application.Repositories;
using TollCalculator.Application.Services;

namespace TollCalculator.Application.Services.Implementations;

internal sealed class TollService : ITollService
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly ITollPassageRepository _tollPassageRepository;
    private readonly ITollScheduleRepository _tollScheduleRepository;
    private readonly IDailyTollService _dailyTollService;
    private readonly ILogger<TollService> _logger;

    public TollService(
        IVehicleRepository vehicleRepository,
        ITollPassageRepository tollPassageRepository,
        ITollScheduleRepository tollScheduleRepository,
        IDailyTollService dailyTollService,
        ILogger<TollService> logger)
    {
        ArgumentNullException.ThrowIfNull(vehicleRepository);
        ArgumentNullException.ThrowIfNull(tollPassageRepository);
        ArgumentNullException.ThrowIfNull(tollScheduleRepository);
        ArgumentNullException.ThrowIfNull(dailyTollService);
        ArgumentNullException.ThrowIfNull(logger);

        _vehicleRepository = vehicleRepository;
        _tollPassageRepository = tollPassageRepository;
        _tollScheduleRepository = tollScheduleRepository;
        _dailyTollService = dailyTollService;
        _logger = logger;
    }

    public async Task<ApplicationResult<DailyToll>> GetDailyTollAsync(
        string registrationNumber,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleRepository.GetAsync(registrationNumber, cancellationToken);

        if (vehicle is null)
        {
            _logger.LogInformation(
                "No vehicle is registered with {RegistrationNumber}; the daily toll cannot be calculated.",
                registrationNumber);

            return ApplicationResult<DailyToll>.NotFound(
                $"No vehicle is registered with the number '{RegistrationNumbers.Normalize(registrationNumber)}'.");
        }

        var passages = await _tollPassageRepository.GetForDayAsync(registrationNumber, date, cancellationToken);
        var timestamps = Array.ConvertAll(passages, passage => passage.PassedAt);

        return await CalculateTollAsync(vehicle.VehicleType, timestamps, cancellationToken);
    }

    private async Task<ApplicationResult<DailyToll>> CalculateTollAsync(
        VehicleType vehicleType,
        DateTimeOffset[] passedAt,
        CancellationToken cancellationToken)
    {
        var tariff = await _tollScheduleRepository.GetTariffAsync(cancellationToken);
        return _dailyTollService.Calculate(vehicleType, passedAt, tariff);
    }
}
