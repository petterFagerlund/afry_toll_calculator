using TollCalculator.Domain.Enums;
using TollCalculator.Domain.Models;
using TollCalculator.Domain.Services;
using TollCalculator.Domain.Services.Implementations;
using Xunit;

namespace TollCalculator.Domain.UnitTests.Services;

public class TollFeeServiceTests
{
    private static readonly TimeSpan Offset = TimeSpan.FromHours(2);
    private readonly ITollFeeService _service;
    private readonly TollSchedule _schedule;

    public TollFeeServiceTests()
    {
        _service = new TollFeeService(new WeekendAndHolidayService(
        [
            Holiday.Create(new DateOnly(2026, 1, 1), "Nyårsdagen")
        ]));

        _schedule = TollSchedule.Create(
        [
            TollRateInterval.Create(new TimeOnly(6, 0), new TimeOnly(6, 29), 8),
            TollRateInterval.Create(new TimeOnly(7, 0), new TimeOnly(7, 59), 18),
            TollRateInterval.Create(new TimeOnly(15, 30), new TimeOnly(16, 59), 18)
        ]);
    }

    [Fact]
    public void CalculateFee_ChargeableVehicleOnWeekday_ReturnsScheduledFee()
    {
        var vehicle = Vehicle.Create("ABC123", VehicleType.Car);
        var passedAt = new DateTimeOffset(2026, 10, 23, 7, 15, 0, Offset);

        Assert.Equal(18, _service.CalculateFee(vehicle, passedAt, _schedule));
    }

    [Fact]
    public void CalculateFee_TollFreeVehicle_ReturnsZero()
    {
        var vehicle = Vehicle.Create("ABC123", VehicleType.Motorbike);
        var passedAt = new DateTimeOffset(2026, 10, 23, 7, 15, 0, Offset);

        Assert.Equal(0, _service.CalculateFee(vehicle, passedAt, _schedule));
    }

    [Fact]
    public void CalculateFee_Weekend_ReturnsZero()
    {
        var vehicle = Vehicle.Create("ABC123", VehicleType.Car);
        var passedAt = new DateTimeOffset(2026, 10, 24, 7, 15, 0, Offset);

        Assert.Equal(0, _service.CalculateFee(vehicle, passedAt, _schedule));
    }

    [Fact]
    public void CalculateFee_Holiday_ReturnsZero()
    {
        var vehicle = Vehicle.Create("ABC123", VehicleType.Car);
        var passedAt = new DateTimeOffset(2026, 1, 1, 7, 15, 0, Offset);

        Assert.Equal(0, _service.CalculateFee(vehicle, passedAt, _schedule));
    }

    [Fact]
    public void CalculateFee_TimeOutsideEveryInterval_ReturnsZero()
    {
        var vehicle = Vehicle.Create("ABC123", VehicleType.Car);
        var passedAt = new DateTimeOffset(2026, 10, 23, 3, 0, 0, Offset);

        Assert.Equal(0, _service.CalculateFee(vehicle, passedAt, _schedule));
    }

    [Fact]
    public void CalculateFee_PassageJustBeforeMidnightLocalTime_UsesLocalDate()
    {
        var vehicle = Vehicle.Create("ABC123", VehicleType.Car);
        var fridayLateEvening = new DateTimeOffset(2026, 10, 23, 23, 30, 0, Offset);

        Assert.Equal(0, _service.CalculateFee(vehicle, fridayLateEvening, _schedule));
    }

    [Fact]
    public void CalculateFee_NullVehicle_Throws()
    {
        var passedAt = new DateTimeOffset(2026, 10, 23, 7, 15, 0, Offset);

        Assert.Throws<ArgumentNullException>(() => _service.CalculateFee(null!, passedAt, _schedule));
    }

    [Fact]
    public void CalculateFee_NullSchedule_Throws()
    {
        var vehicle = Vehicle.Create("ABC123", VehicleType.Car);
        var passedAt = new DateTimeOffset(2026, 10, 23, 7, 15, 0, Offset);

        Assert.Throws<ArgumentNullException>(() => _service.CalculateFee(vehicle, passedAt, null!));
    }
}
