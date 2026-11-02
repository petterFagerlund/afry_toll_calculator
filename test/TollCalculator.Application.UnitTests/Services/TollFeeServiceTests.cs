using TollCalculator.Domain.Enums;
using TollCalculator.Domain.Models;
using TollCalculator.Application.Services;
using TollCalculator.Application.Services.Implementations;

namespace TollCalculator.Application.UnitTests.Services;

public class TollFeeServiceTests
{
    private static readonly TimeSpan Offset = TimeSpan.FromHours(2);
    private static readonly Holiday[] Holidays = [Holiday.Create(new DateOnly(2026, 1, 1), "Nyårsdagen")];
    private readonly ITollFeeService _service;
    private readonly TollSchedule _schedule;

    public TollFeeServiceTests()
    {
        _service = new TollFeeService(new WeekendAndHolidayService(new StubHolidayProvider(Holidays)));

        _schedule = TollSchedule.Create(
        [
            TollRateInterval.Create(new TimeOnly(6, 0), new TimeOnly(6, 30), 8),
            TollRateInterval.Create(new TimeOnly(7, 0), new TimeOnly(8, 0), 18),
            TollRateInterval.Create(new TimeOnly(15, 30), new TimeOnly(17, 0), 18)
        ]);
    }

    [Fact]
    public void CalculateFee_ChargeableVehicleOnWeekday_ReturnsScheduledFee()
    {
        var vehicleType = VehicleType.Car;
        var passedAt = new DateTimeOffset(2026, 10, 23, 7, 15, 0, Offset);

        Assert.Equal(18, _service.CalculateFee(vehicleType, passedAt, _schedule).Value);
    }

    [Fact]
    public void CalculateFee_TollFreeVehicle_ReturnsZero()
    {
        var vehicleType = VehicleType.Motorbike;
        var passedAt = new DateTimeOffset(2026, 10, 23, 7, 15, 0, Offset);

        Assert.Equal(0, _service.CalculateFee(vehicleType, passedAt, _schedule).Value);
    }

    [Fact]
    public void CalculateFee_Weekend_ReturnsZero()
    {
        var vehicleType = VehicleType.Car;
        var passedAt = new DateTimeOffset(2026, 10, 24, 7, 15, 0, Offset);

        Assert.Equal(0, _service.CalculateFee(vehicleType, passedAt, _schedule).Value);
    }

    [Fact]
    public void CalculateFee_Holiday_ReturnsZero()
    {
        var vehicleType = VehicleType.Car;
        var passedAt = new DateTimeOffset(2026, 1, 1, 7, 15, 0, Offset);

        Assert.Equal(0, _service.CalculateFee(vehicleType, passedAt, _schedule).Value);
    }

    [Theory]
    [InlineData(6, 29, 30, 8)]
    [InlineData(6, 29, 59, 8)]
    [InlineData(7, 59, 59, 18)]
    public void CalculateFee_SecondsInsideAnInterval_ReturnsThatIntervalsFee(
        int hour,
        int minute,
        int second,
        int expected)
    {
        var vehicleType = VehicleType.Car;
        var passedAt = new DateTimeOffset(2026, 10, 23, hour, minute, second, Offset);

        Assert.Equal(expected, _service.CalculateFee(vehicleType, passedAt, _schedule).Value);
    }

    [Fact]
    public void CalculateFee_TimeOutsideEveryInterval_ReturnsZero()
    {
        var vehicleType = VehicleType.Car;
        var passedAt = new DateTimeOffset(2026, 10, 23, 3, 0, 0, Offset);

        Assert.Equal(0, _service.CalculateFee(vehicleType, passedAt, _schedule).Value);
    }

    [Fact]
    public void CalculateFee_PassageJustBeforeMidnightLocalTime_UsesLocalDate()
    {
        var vehicleType = VehicleType.Car;
        var fridayLateEvening = new DateTimeOffset(2026, 10, 23, 23, 30, 0, Offset);

        Assert.Equal(0, _service.CalculateFee(vehicleType, fridayLateEvening, _schedule).Value);
    }

    [Fact]
    public void CalculateFee_SameInstantWrittenInAnotherOffset_ChargesTheSameFee()
    {
        var vehicleType = VehicleType.Car;
        var stockholmNotation = new DateTimeOffset(2026, 10, 23, 7, 15, 0, Offset);
        var utcNotation = stockholmNotation.ToUniversalTime();

        Assert.Equal(
            _service.CalculateFee(vehicleType, stockholmNotation, _schedule).Value,
            _service.CalculateFee(vehicleType, utcNotation, _schedule).Value);
    }

    [Fact]
    public void CalculateFee_PassageGivenInUtc_UsesTheStockholmTimeOfDay()
    {
        var vehicleType = VehicleType.Car;
        var utcNotation = new DateTimeOffset(2026, 10, 23, 5, 15, 0, TimeSpan.Zero);

        Assert.Equal(18, _service.CalculateFee(vehicleType, utcNotation, _schedule).Value);
    }

    [Fact]
    public void CalculateFee_NullSchedule_Throws()
    {
        var vehicleType = VehicleType.Car;
        var passedAt = new DateTimeOffset(2026, 10, 23, 7, 15, 0, Offset);

        Assert.Throws<ArgumentNullException>(() => _service.CalculateFee(vehicleType, passedAt, null!).Value);
    }
}
