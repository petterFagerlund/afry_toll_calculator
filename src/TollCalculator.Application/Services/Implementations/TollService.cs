using TollCalculator.Application.Results;
using TollCalculator.Domain.Enums;
using TollCalculator.Domain.Models;
using TollCalculator.Infrastructure.Repositories;

namespace TollCalculator.Application.Services.Implementations;

internal sealed class TollService : ITollService
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly ITollPassageRepository _tollPassageRepository;
    private readonly ITollScheduleRepository _tollScheduleRepository;
    private readonly IHolidayRepository _holidayRepository;
    private readonly IDailyTollServiceFactory _dailyTollServiceFactory;

    public TollService(
        IVehicleRepository vehicleRepository,
        ITollPassageRepository tollPassageRepository,
        ITollScheduleRepository tollScheduleRepository,
        IHolidayRepository holidayRepository,
        IDailyTollServiceFactory dailyTollServiceFactory)
    {
        ArgumentNullException.ThrowIfNull(vehicleRepository);
        ArgumentNullException.ThrowIfNull(tollPassageRepository);
        ArgumentNullException.ThrowIfNull(tollScheduleRepository);
        ArgumentNullException.ThrowIfNull(holidayRepository);
        ArgumentNullException.ThrowIfNull(dailyTollServiceFactory);

        _vehicleRepository = vehicleRepository;
        _tollPassageRepository = tollPassageRepository;
        _tollScheduleRepository = tollScheduleRepository;
        _holidayRepository = holidayRepository;
        _dailyTollServiceFactory = dailyTollServiceFactory;
    }

    public async Task<ApplicationResult<DailyToll>> GetDailyTollAsync(
        string registrationNumber,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(registrationNumber))
        {
            return ApplicationResult<DailyToll>.Invalid("A registration number is required.");
        }

        var vehicle = await _vehicleRepository.GetAsync(registrationNumber, cancellationToken);

        if (vehicle is null)
        {
            return ApplicationResult<DailyToll>.NotFound(
                $"No vehicle is registered with the number '{registrationNumber.Trim().ToUpperInvariant()}'.");
        }

        var passages = await _tollPassageRepository.GetForDayAsync(registrationNumber, date, cancellationToken);
        var holidays = await _holidayRepository.GetByYearAsync(date.Year, cancellationToken);

        return await CalculateDailyTollAsync(vehicle, passages, holidays, cancellationToken);
    }

    public async Task<ApplicationResult<DailyToll>> CalculateAsync(
        string vehicleType,
        DateTimeOffset[] timestamps,
        CancellationToken cancellationToken)
    {
        if (!VehicleTypes.TryParse(vehicleType, out var parsedVehicleType))
        {
            return ApplicationResult<DailyToll>.Invalid($"'{vehicleType}' is not a known vehicle type.");
        }

        if (timestamps is null)
        {
            return ApplicationResult<DailyToll>.Invalid("A list of passage timestamps is required.");
        }

        var vehicle = Vehicle.Unregistered(parsedVehicleType);
        var passages = new TollPassage[timestamps.Length];

        for (var i = 0; i < timestamps.Length; i++)
        {
            passages[i] = TollPassage.Create(vehicle.RegistrationNumber, timestamps[i]);
        }

        var holidays = await _holidayRepository.GetAllAsync(cancellationToken);

        return await CalculateDailyTollAsync(vehicle, passages, holidays, cancellationToken);
    }

    private async Task<ApplicationResult<DailyToll>> CalculateDailyTollAsync(
        Vehicle vehicle,
        TollPassage[] passages,
        Holiday[] holidays,
        CancellationToken cancellationToken)
    {
        var schedule = await _tollScheduleRepository.GetScheduleAsync(cancellationToken);
        var dailyCap = await _tollScheduleRepository.GetDailyCapAsync(cancellationToken);

        var dailyTollService = _dailyTollServiceFactory.Create(holidays);

        return ApplicationResult<DailyToll>.Success(dailyTollService.Calculate(vehicle, passages, schedule, dailyCap));
    }
}
