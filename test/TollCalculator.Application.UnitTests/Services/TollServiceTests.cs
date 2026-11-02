using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using TollCalculator.Application.Providers;
using TollCalculator.Application.Results;
using TollCalculator.Application.Services.Implementations;
using TollCalculator.Domain.Enums;
using TollCalculator.Domain.Models;
using TollCalculator.Application.Repositories;
using TollCalculator.Application.Services;
using TollCalculator.Application.Services.Implementations;
using Xunit;

namespace TollCalculator.Application.UnitTests.Services;

public class TollServiceTests
{
    private static readonly DateOnly Monday = new(2026, 3, 16);

    private readonly IVehicleRepository _vehicleRepository = Substitute.For<IVehicleRepository>();
    private readonly ITollPassageRepository _tollPassageRepository = Substitute.For<ITollPassageRepository>();
    private readonly ITollScheduleRepository _tollScheduleRepository = Substitute.For<ITollScheduleRepository>();
    private readonly IHolidayProvider _holidayProvider = Substitute.For<IHolidayProvider>();
    private readonly TollService _service;

    public TollServiceTests()
    {
        _tollScheduleRepository.GetTariffAsync(Arg.Any<CancellationToken>())
            .Returns(TollTariff.Create(StandardSchedule(), 60));

        _holidayProvider.GetHolidays(Arg.Any<int>()).Returns(ApplicationResult<Holiday[]>.Success([]));

        _service = new TollService(
            _vehicleRepository,
            _tollPassageRepository,
            _tollScheduleRepository,
            new DailyTollService(new TollFeeService(new WeekendAndHolidayService(_holidayProvider))),
            NullLogger<TollService>.Instance);
    }

    [Fact]
    public async Task GetDailyTollAsync_DayWithPassages_ReturnsTheTotalAndBreakdown()
    {
        KnownVehicle("ABC123", VehicleType.Car);
        PassagesOn(Monday, At(Monday, 7, 15), At(Monday, 15, 10));

        var result = await _service.GetDailyTollAsync("ABC123", Monday, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(31, result.Value!.TotalFee);
        Assert.Equal(2, result.Value.ChargedPassages.Length);
        Assert.Equal(18, result.Value.ChargedPassages[0].Amount);
        Assert.Equal(13, result.Value.ChargedPassages[1].Amount);
    }

    [Fact]
    public async Task GetDailyTollAsync_DayWithNoPassages_ReturnsZeroAndAnEmptyBreakdown()
    {
        KnownVehicle("ABC123", VehicleType.Car);
        PassagesOn(Monday);

        var result = await _service.GetDailyTollAsync("ABC123", Monday, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value!.TotalFee);
        Assert.Empty(result.Value.ChargedPassages);
    }

    [Fact]
    public async Task GetDailyTollAsync_UnknownVehicle_ReturnsNotFound()
    {
        _vehicleRepository.GetAsync("XYZ789", Arg.Any<CancellationToken>()).Returns((Vehicle?)null);

        var result = await _service.GetDailyTollAsync("XYZ789", Monday, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationResultStatus.NotFound, result.Status);
        Assert.Contains("XYZ789", result.ErrorMessage);
    }


    private void KnownVehicle(string registrationNumber, VehicleType vehicleType)
    {
        var vehicle = Vehicle.Create(registrationNumber, vehicleType);

        _vehicleRepository.GetAsync(registrationNumber, Arg.Any<CancellationToken>()).Returns(vehicle);
    }

    private void PassagesOn(DateOnly date, params DateTimeOffset[] timestamps)
    {
        var passages = new List<TollPassage>();

        foreach (var timestamp in timestamps)
        {
            passages.Add(TollPassage.Create("ABC123", timestamp));
        }

        _tollPassageRepository.GetForDayAsync("ABC123", date, Arg.Any<CancellationToken>()).Returns(passages.ToArray());
    }

    private static DateTimeOffset At(DateOnly date, int hour, int minute)
    {
        var local = date.ToDateTime(new TimeOnly(hour, minute));

        return new DateTimeOffset(local, TollCalculator.Domain.Constants.TollTimeZone.Instance.GetUtcOffset(local));
    }

    private static TollSchedule StandardSchedule()
    {
        return TollSchedule.Create(
        [
            TollRateInterval.Create(new TimeOnly(6, 0), new TimeOnly(6, 29), 8),
            TollRateInterval.Create(new TimeOnly(6, 30), new TimeOnly(6, 59), 13),
            TollRateInterval.Create(new TimeOnly(7, 0), new TimeOnly(7, 59), 18),
            TollRateInterval.Create(new TimeOnly(8, 0), new TimeOnly(8, 29), 13),
            TollRateInterval.Create(new TimeOnly(8, 30), new TimeOnly(14, 59), 8),
            TollRateInterval.Create(new TimeOnly(15, 0), new TimeOnly(15, 29), 13),
            TollRateInterval.Create(new TimeOnly(15, 30), new TimeOnly(16, 59), 18),
            TollRateInterval.Create(new TimeOnly(17, 0), new TimeOnly(17, 59), 13),
            TollRateInterval.Create(new TimeOnly(18, 0), new TimeOnly(18, 29), 8)
        ]);
    }
}
