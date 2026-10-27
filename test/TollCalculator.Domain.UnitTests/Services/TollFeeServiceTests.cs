using TollCalculator.Domain.Enums;
using TollCalculator.Domain.Models;
using TollCalculator.Domain.Services;
using TollCalculator.Domain.Services.Implementations;
using Xunit;

namespace TollCalculator.Domain.UnitTests.Services;

public class TollFeeServiceTests
{
    private static readonly TimeSpan Offset = TimeSpan.FromHours(2);
    private static readonly Holiday[] Holidays = [Holiday.Create(new DateOnly(2026, 1, 1), "Nyårsdagen")];
    private readonly ITollFeeService _service;
    private readonly TollSchedule _schedule;

    public TollFeeServiceTests()
    {
        _service = new TollFeeService(new WeekendAndHolidayService());

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

        Assert.Equal(18, _service.CalculateFee(vehicle, passedAt, _schedule, Holidays));
    }

    [Fact]
    public void CalculateFee_TollFreeVehicle_ReturnsZero()
    {
        var vehicle = Vehicle.Create("ABC123", VehicleType.Motorbike);
        var passedAt = new DateTimeOffset(2026, 10, 23, 7, 15, 0, Offset);

        Assert.Equal(0, _service.CalculateFee(vehicle, passedAt, _schedule, Holidays));
    }

    [Fact]
    public void CalculateFee_Weekend_ReturnsZero()
    {
        var vehicle = Vehicle.Create("ABC123", VehicleType.Car);
        var passedAt = new DateTimeOffset(2026, 10, 24, 7, 15, 0, Offset);

        Assert.Equal(0, _service.CalculateFee(vehicle, passedAt, _schedule, Holidays));
    }

    [Fact]
    public void CalculateFee_Holiday_ReturnsZero()
    {
        var vehicle = Vehicle.Create("ABC123", VehicleType.Car);
        var passedAt = new DateTimeOffset(2026, 1, 1, 7, 15, 0, Offset);

        Assert.Equal(0, _service.CalculateFee(vehicle, passedAt, _schedule, Holidays));
    }

    [Fact]
    public void CalculateFee_TimeOutsideEveryInterval_ReturnsZero()
    {
        var vehicle = Vehicle.Create("ABC123", VehicleType.Car);
        var passedAt = new DateTimeOffset(2026, 10, 23, 3, 0, 0, Offset);

        Assert.Equal(0, _service.CalculateFee(vehicle, passedAt, _schedule, Holidays));
    }

    [Fact]
    public void CalculateFee_PassageJustBeforeMidnightLocalTime_UsesLocalDate()
    {
        var vehicle = Vehicle.Create("ABC123", VehicleType.Car);
        var fridayLateEvening = new DateTimeOffset(2026, 10, 23, 23, 30, 0, Offset);

        Assert.Equal(0, _service.CalculateFee(vehicle, fridayLateEvening, _schedule, Holidays));
    }

    [Fact]
    public void CalculateFee_SameInstantWrittenInAnotherOffset_ChargesTheSameFee()
    {
        var vehicle = Vehicle.Create("ABC123", VehicleType.Car);
        var stockholmNotation = new DateTimeOffset(2026, 10, 23, 7, 15, 0, Offset);
        var utcNotation = stockholmNotation.ToUniversalTime();

        Assert.Equal(
            _service.CalculateFee(vehicle, stockholmNotation, _schedule, Holidays),
            _service.CalculateFee(vehicle, utcNotation, _schedule, Holidays));
    }

    [Fact]
    public void CalculateFee_PassageGivenInUtc_UsesTheStockholmTimeOfDay()
    {
        var vehicle = Vehicle.Create("ABC123", VehicleType.Car);
        var utcNotation = new DateTimeOffset(2026, 10, 23, 5, 15, 0, TimeSpan.Zero);

        Assert.Equal(18, _service.CalculateFee(vehicle, utcNotation, _schedule, Holidays));
    }

    [Fact]
    public void CalculateFee_NullVehicle_Throws()
    {
        var passedAt = new DateTimeOffset(2026, 10, 23, 7, 15, 0, Offset);

        Assert.Throws<ArgumentNullException>(() => _service.CalculateFee(null!, passedAt, _schedule, Holidays));
    }

    [Fact]
    public void CalculateFee_NullSchedule_Throws()
    {
        var vehicle = Vehicle.Create("ABC123", VehicleType.Car);
        var passedAt = new DateTimeOffset(2026, 10, 23, 7, 15, 0, Offset);

        Assert.Throws<ArgumentNullException>(() => _service.CalculateFee(vehicle, passedAt, null!, Holidays));
    }
}
