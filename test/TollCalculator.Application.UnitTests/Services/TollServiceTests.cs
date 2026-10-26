using NSubstitute;
using TollCalculator.Application.Services.Implementations;
using TollCalculator.Domain.Enums;
using TollCalculator.Domain.Models;
using TollCalculator.Infrastructure.Repositories;
using Xunit;

namespace TollCalculator.Application.UnitTests.Services;

public class TollServiceTests
{
    private static readonly DateOnly Monday = new(2026, 3, 16);

    private readonly IVehicleRepository _vehicleRepository = Substitute.For<IVehicleRepository>();
    private readonly ITollPassageRepository _tollPassageRepository = Substitute.For<ITollPassageRepository>();
    private readonly ITollScheduleRepository _tollScheduleRepository = Substitute.For<ITollScheduleRepository>();
    private readonly IHolidayRepository _holidayRepository = Substitute.For<IHolidayRepository>();
    private readonly TollService _service;

    public TollServiceTests()
    {
        _tollScheduleRepository.GetScheduleAsync(Arg.Any<CancellationToken>()).Returns(StandardSchedule());
        _tollScheduleRepository.GetDailyCapAsync(Arg.Any<CancellationToken>()).Returns(60);
        _holidayRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns([]);
        _holidayRepository.GetByYearAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns([]);

        _service = new TollService(
            _vehicleRepository,
            _tollPassageRepository,
            _tollScheduleRepository,
            _holidayRepository,
            new DailyTollServiceFactory());
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
        Assert.True(result.IsNotFound);
        Assert.Contains("XYZ789", result.ErrorMessage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task GetDailyTollAsync_BlankRegistrationNumber_ReturnsInvalid(string? registrationNumber)
    {
        var result = await _service.GetDailyTollAsync(registrationNumber!, Monday, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsInvalid);
    }

    [Fact]
    public async Task CalculateAsync_KnownVehicleType_ChargesTheHighestFeeWithinTheHourlyWindow()
    {
        var result = await _service.CalculateAsync(
            "Car",
            [At(Monday, 7, 15), At(Monday, 7, 45)],
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(18, result.Value!.TotalFee);
        Assert.Single(result.Value.ChargedPassages);
    }

    [Fact]
    public async Task CalculateAsync_TollFreeVehicleType_ReturnsZero()
    {
        var result = await _service.CalculateAsync("Emergency", [At(Monday, 7, 15)], CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value!.TotalFee);
    }

    [Theory]
    [InlineData("Spaceship")]
    [InlineData("Unknown")]
    [InlineData("")]
    [InlineData(null)]
    public async Task CalculateAsync_UnknownVehicleTypeString_ReturnsInvalid(string? vehicleType)
    {
        var result = await _service.CalculateAsync(vehicleType!, [At(Monday, 7, 15)], CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsInvalid);
    }

    [Fact]
    public async Task CalculateAsync_EmptyTimestamps_ReturnsZeroAndAnEmptyBreakdown()
    {
        var result = await _service.CalculateAsync("Car", [], CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value!.TotalFee);
        Assert.Empty(result.Value.ChargedPassages);
    }

    [Fact]
    public async Task CalculateAsync_PersistsNothing()
    {
        await _service.CalculateAsync("Car", [At(Monday, 7, 15)], CancellationToken.None);

        await _tollPassageRepository.DidNotReceiveWithAnyArgs().AddAsync(null!, CancellationToken.None);
    }

    [Fact]
    public async Task CalculateAsync_HolidayTimestamps_AreNotCharged()
    {
        var goodFriday = new DateOnly(2026, 4, 3);
        _holidayRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns([Holiday.Create(goodFriday, "Långfredagen")]);

        var result = await _service.CalculateAsync("Car", [At(goodFriday, 7, 15)], CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value!.TotalFee);
    }

    private void KnownVehicle(string registrationNumber, VehicleType vehicleType)
    {
        var vehicle = Vehicle.Create(registrationNumber, vehicleType);

        _vehicleRepository.GetAsync(registrationNumber, Arg.Any<CancellationToken>()).Returns(vehicle);
    }

    private void PassagesOn(DateOnly date, params DateTimeOffset[] timestamps)
    {
        var passages = new TollPassage[timestamps.Length];

        for (var i = 0; i < timestamps.Length; i++)
        {
            passages[i] = TollPassage.Create("ABC123", timestamps[i]);
        }

        _tollPassageRepository.GetForDayAsync("ABC123", date, Arg.Any<CancellationToken>()).Returns(passages);
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
