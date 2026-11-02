using TollCalculator.Domain.Enums;
using TollCalculator.Domain.Models;
using TollCalculator.Domain.Services;
using TollCalculator.Domain.Services.Implementations;
using Xunit;

namespace TollCalculator.Domain.UnitTests.Services;

public class DailyTollServiceTests
{
    private const int DailyCap = 60;
    private static readonly TimeSpan Offset = TimeSpan.FromHours(2);
    private static readonly DateOnly Weekday = new(2026, 10, 19);
    private static readonly Holiday[] Holidays = [Holiday.Create(new DateOnly(2026, 1, 1), "Nyårsdagen")];
    private readonly IDailyTollService _service;
    private readonly TollTariff _tariff;

    public DailyTollServiceTests()
    {
        var tollFeeService = new TollFeeService(new WeekendAndHolidayService(new StubHolidayProvider(Holidays)));

        _service = new DailyTollService(tollFeeService);
        _tariff = TollTariff.Create(TestSchedules.Standard(), DailyCap);
    }

    [Fact]
    public void Calculate_NoPassages_ReturnsZero()
    {
        var result = _service.Calculate(VehicleType.Car, [], _tariff);

        Assert.Equal(0, result.TotalFee);
        Assert.Empty(result.ChargedPassages);
    }

    [Fact]
    public void Calculate_UnsortedPassages_ChargesTheSameAsSorted()
    {
        DateTimeOffset[] unsorted = [PassageAt(15, 35), PassageAt(7, 15)];

        var result = _service.Calculate(VehicleType.Car, unsorted, _tariff);

        Assert.Equal(36, result.TotalFee);
        Assert.Equal(PassageAt(7, 15), result.ChargedPassages[0].PassedAt);
    }

    [Fact]
    public void Calculate_TwoPassagesWithinTheSameHour_ChargesOnlyTheHighest()
    {
        DateTimeOffset[] passages = [PassageAt(6, 15), PassageAt(6, 35)];

        var result = _service.Calculate(VehicleType.Car, passages, _tariff);

        Assert.Equal(13, result.TotalFee);
        Assert.Single(result.ChargedPassages);
        Assert.Equal(PassageAt(6, 35), result.ChargedPassages[0].PassedAt);
    }

    [Fact]
    public void Calculate_TwoPassagesMoreThanAnHourApart_ChargesBoth()
    {
        DateTimeOffset[] passages = [PassageAt(6, 15), PassageAt(7, 25)];

        var result = _service.Calculate(VehicleType.Car, passages, _tariff);

        Assert.Equal(26, result.TotalFee);
        Assert.Equal(2, result.ChargedPassages.Length);
    }

    [Fact]
    public void Calculate_PassagesExactlySixtyMinutesApart_ChargesBoth()
    {
        DateTimeOffset[] passages = [PassageAt(6, 0), PassageAt(7, 0)];

        var result = _service.Calculate(VehicleType.Car, passages, _tariff);

        Assert.Equal(26, result.TotalFee);
        Assert.Equal(2, result.ChargedPassages.Length);
    }

    [Fact]
    public void Calculate_DayExceedingTheCap_StopsAtTheCap()
    {
        DateTimeOffset[] passages =
        [
            PassageAt(6, 0), PassageAt(7, 5), PassageAt(8, 10), PassageAt(9, 15), PassageAt(15, 35)
        ];

        var result = _service.Calculate(VehicleType.Car, passages, _tariff);

        Assert.Equal(DailyCap, result.TotalFee);
        Assert.Equal(DailyCap, result.ChargedPassages.Sum(charged => charged.Amount));
    }

    [Fact]
    public void Calculate_TollFreeVehicle_ReturnsZero()
    {
        var result = _service.Calculate(VehicleType.Motorbike, [PassageAt(7, 15)], _tariff);

        Assert.Equal(0, result.TotalFee);
        Assert.Empty(result.ChargedPassages);
    }

    [Fact]
    public void Calculate_Holiday_ReturnsZero()
    {
        var onNewYearsDay = new DateTimeOffset(2026, 1, 1, 7, 15, 0, Offset);

        var result = _service.Calculate(VehicleType.Car, [onNewYearsDay], _tariff);

        Assert.Equal(0, result.TotalFee);
        Assert.Empty(result.ChargedPassages);
    }

    [Fact]
    public void Calculate_PassagesOutsideChargedHours_AreNotCharged()
    {
        DateTimeOffset[] passages = [PassageAt(3, 0), PassageAt(22, 0)];

        var result = _service.Calculate(VehicleType.Car, passages, _tariff);

        Assert.Equal(0, result.TotalFee);
        Assert.Empty(result.ChargedPassages);
    }

    [Fact]
    public void Calculate_FreePassageBeforeACharged_DoesNotOpenTheWindow()
    {
        DateTimeOffset[] passages = [PassageAt(5, 30), PassageAt(6, 15), PassageAt(6, 45)];

        var result = _service.Calculate(VehicleType.Car, passages, _tariff);

        Assert.Equal(13, result.TotalFee);
        Assert.Single(result.ChargedPassages);
    }

    [Fact]
    public void Calculate_PassagesOnTwoDays_AppliesTheCapPerDay()
    {
        DateTimeOffset[] passages =
        [
            PassageAt(6, 0), PassageAt(7, 5), PassageAt(8, 10), PassageAt(9, 15), PassageAt(15, 35),
            PassageOnNextDayAt(6, 0), PassageOnNextDayAt(7, 5), PassageOnNextDayAt(8, 10),
            PassageOnNextDayAt(9, 15), PassageOnNextDayAt(15, 35)
        ];

        var result = _service.Calculate(VehicleType.Car, passages, _tariff);

        Assert.Equal(DailyCap * 2, result.TotalFee);
    }

    [Fact]
    public void Calculate_OnePassageOnEachOfTwoDays_ChargesBoth()
    {
        DateTimeOffset[] passages = [PassageAt(7, 15), PassageOnNextDayAt(7, 15)];

        var result = _service.Calculate(VehicleType.Car, passages, _tariff);

        Assert.Equal(36, result.TotalFee);
        Assert.Equal(2, result.ChargedPassages.Length);
    }

    [Fact]
    public void Calculate_NullPassages_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => _service.Calculate(VehicleType.Car, null!, _tariff));
    }

    private static DateTimeOffset PassageAt(int hour, int minute)
    {
        return PassageOn(Weekday, hour, minute);
    }

    private static DateTimeOffset PassageOnNextDayAt(int hour, int minute)
    {
        return PassageOn(Weekday.AddDays(1), hour, minute);
    }

    private static DateTimeOffset PassageOn(DateOnly date, int hour, int minute)
    {
        return new DateTimeOffset(date.Year, date.Month, date.Day, hour, minute, 0, Offset);
    }
}
