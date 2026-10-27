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
    private readonly IHolidayRepository _holidayRepository;
    private readonly IDailyTollService _dailyTollService;

    public TollService(
        IVehicleRepository vehicleRepository,
        ITollPassageRepository tollPassageRepository,
        ITollScheduleRepository tollScheduleRepository,
        IHolidayRepository holidayRepository,
        IDailyTollService dailyTollService)
    {
        ArgumentNullException.ThrowIfNull(vehicleRepository);
        ArgumentNullException.ThrowIfNull(tollPassageRepository);
        ArgumentNullException.ThrowIfNull(tollScheduleRepository);
        ArgumentNullException.ThrowIfNull(holidayRepository);
        ArgumentNullException.ThrowIfNull(dailyTollService);

        _vehicleRepository = vehicleRepository;
        _tollPassageRepository = tollPassageRepository;
        _tollScheduleRepository = tollScheduleRepository;
        _holidayRepository = holidayRepository;
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
        var holidays = await _holidayRepository.GetByYearAsync(date.Year, cancellationToken);

        return await CalculateTollAsync(vehicle, passages, holidays, cancellationToken);
    }

    public async Task<ApplicationResult<DailyToll>> CalculateAsync(
        string vehicleType,
        DateTimeOffset[] timestamps,
        CancellationToken cancellationToken)
    {
        if (!VehicleTypeParser.TryParse(vehicleType, out var parsedVehicleType))
        {
            return ApplicationResult<DailyToll>.Invalid($"'{vehicleType}' is not a known vehicle type.");
        }

        var vehicle = Vehicle.Unregistered(parsedVehicleType);
        var passages = new TollPassage[timestamps.Length];

        for (var i = 0; i < timestamps.Length; i++)
        {
            passages[i] = TollPassage.Create(vehicle.RegistrationNumber, timestamps[i]);
        }

        var holidays = await _holidayRepository.GetAllAsync(cancellationToken);

        return await CalculateTollAsync(vehicle, passages, holidays, cancellationToken);
    }

    private async Task<ApplicationResult<DailyToll>> CalculateTollAsync(
        Vehicle vehicle,
        TollPassage[] passages,
        Holiday[] holidays,
        CancellationToken cancellationToken)
    {
        var tariff = await _tollScheduleRepository.GetTariffAsync(cancellationToken);

        return ApplicationResult<DailyToll>.Success(_dailyTollService.Calculate(vehicle, passages, tariff, holidays));
    }
}
