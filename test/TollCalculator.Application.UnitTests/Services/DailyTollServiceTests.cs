using NSubstitute;
using TollCalculator.Application.Repositories;
using TollCalculator.Application.Services;
using TollCalculator.Application.Services.Implementations;
using TollCalculator.Domain.Enums;
using TollCalculator.Domain.Models;

namespace TollCalculator.Application.UnitTests.Services;

public class DailyTollServiceTests
{
    private const int DailyCap = 60;
    private static readonly TimeSpan Offset = TimeSpan.FromHours(2);
    private static readonly DateOnly Weekday = new(2026, 10, 19);
    private static readonly Holiday[] Holidays = [Holiday.Create(new DateOnly(2026, 1, 1), "Nyårsdagen")];
    private readonly ITollScheduleRepository _tollScheduleRepository = Substitute.For<ITollScheduleRepository>();
    private readonly IDailyTollService _service;

    public DailyTollServiceTests()
    {
        var tollFeeService = new TollFeeService(new WeekendAndHolidayService(new StubHolidayProvider(Holidays)));

        _tollScheduleRepository.GetTariffAsync(Arg.Any<CancellationToken>())
            .Returns(TollTariff.Create(TestSchedules.Standard(), DailyCap));

        _service = new DailyTollService(tollFeeService, _tollScheduleRepository);
    }

    [Fact]
    public async Task Calculate_NoPassages_ReturnsZero()
    {
        var result = await _service.Calculate(PassagesOf(VehicleType.Car, []), CancellationToken.None);

        Assert.Equal(0, result.Value!.TotalFee);
        Assert.Empty(result.Value!.ChargedPassages);
    }

    [Fact]
    public async Task Calculate_UnsortedPassages_ChargesTheSameAsSorted()
    {
        DateTimeOffset[] unsorted = [PassageAt(15, 35), PassageAt(7, 15)];

        var result = await _service.Calculate(PassagesOf(VehicleType.Car, unsorted), CancellationToken.None);

        Assert.Equal(36, result.Value!.TotalFee);
        Assert.Equal(PassageAt(7, 15), result.Value!.ChargedPassages[0].PassedAt);
    }

    [Fact]
    public async Task Calculate_TwoPassagesWithinTheSameHour_ChargesOnlyTheHighest()
    {
        DateTimeOffset[] passages = [PassageAt(6, 15), PassageAt(6, 35)];

        var result = await _service.Calculate(PassagesOf(VehicleType.Car, passages), CancellationToken.None);

        Assert.Equal(13, result.Value!.TotalFee);
        Assert.Single(result.Value!.ChargedPassages);
        Assert.Equal(PassageAt(6, 35), result.Value!.ChargedPassages[0].PassedAt);
    }

    [Fact]
    public async Task Calculate_TwoPassagesMoreThanAnHourApart_ChargesBoth()
    {
        DateTimeOffset[] passages = [PassageAt(6, 15), PassageAt(7, 25)];

        var result = await _service.Calculate(PassagesOf(VehicleType.Car, passages), CancellationToken.None);

        Assert.Equal(26, result.Value!.TotalFee);
        Assert.Equal(2, result.Value!.ChargedPassages.Length);
    }

    [Fact]
    public async Task Calculate_PassagesExactlySixtyMinutesApart_ChargesBoth()
    {
        DateTimeOffset[] passages = [PassageAt(6, 0), PassageAt(7, 0)];

        var result = await _service.Calculate(PassagesOf(VehicleType.Car, passages), CancellationToken.None);

        Assert.Equal(26, result.Value!.TotalFee);
        Assert.Equal(2, result.Value!.ChargedPassages.Length);
    }

    [Fact]
    public async Task Calculate_DayExceedingTheCap_StopsAtTheCap()
    {
        DateTimeOffset[] passages =
        [
            PassageAt(6, 0), PassageAt(7, 5), PassageAt(8, 10), PassageAt(9, 15), PassageAt(15, 35)
        ];

        var result = await _service.Calculate(PassagesOf(VehicleType.Car, passages), CancellationToken.None);

        Assert.Equal(DailyCap, result.Value!.TotalFee);
        Assert.Equal(DailyCap, result.Value!.ChargedPassages.Sum(charged => charged.Amount));
    }

    [Fact]
    public async Task Calculate_TollFreeVehicle_ReturnsZero()
    {
        var result = await _service.Calculate(PassagesOf(VehicleType.Motorbike, [PassageAt(7, 15)]), CancellationToken.None);

        Assert.Equal(0, result.Value!.TotalFee);
        Assert.Empty(result.Value!.ChargedPassages);
    }

    [Fact]
    public async Task Calculate_Holiday_ReturnsZero()
    {
        var onNewYearsDay = new DateTimeOffset(2026, 1, 1, 7, 15, 0, Offset);

        var result = await _service.Calculate(PassagesOf(VehicleType.Car, [onNewYearsDay]), CancellationToken.None);

        Assert.Equal(0, result.Value!.TotalFee);
        Assert.Empty(result.Value!.ChargedPassages);
    }

    [Fact]
    public async Task Calculate_PassagesOutsideChargedHours_AreNotCharged()
    {
        DateTimeOffset[] passages = [PassageAt(3, 0), PassageAt(22, 0)];

        var result = await _service.Calculate(PassagesOf(VehicleType.Car, passages), CancellationToken.None);

        Assert.Equal(0, result.Value!.TotalFee);
        Assert.Empty(result.Value!.ChargedPassages);
    }

    [Fact]
    public async Task Calculate_FreePassageBeforeACharged_DoesNotOpenTheWindow()
    {
        DateTimeOffset[] passages = [PassageAt(5, 30), PassageAt(6, 15), PassageAt(6, 45)];

        var result = await _service.Calculate(PassagesOf(VehicleType.Car, passages), CancellationToken.None);

        Assert.Equal(13, result.Value!.TotalFee);
        Assert.Single(result.Value!.ChargedPassages);
    }

    [Fact]
    public async Task Calculate_PassagesOnTwoDays_AppliesTheCapPerDay()
    {
        DateTimeOffset[] passages =
        [
            PassageAt(6, 0), PassageAt(7, 5), PassageAt(8, 10), PassageAt(9, 15), PassageAt(15, 35),
            PassageOnNextDayAt(6, 0), PassageOnNextDayAt(7, 5), PassageOnNextDayAt(8, 10),
            PassageOnNextDayAt(9, 15), PassageOnNextDayAt(15, 35)
        ];

        var result = await _service.Calculate(PassagesOf(VehicleType.Car, passages), CancellationToken.None);

        Assert.Equal(DailyCap * 2, result.Value!.TotalFee);
    }

    [Fact]
    public async Task Calculate_OnePassageOnEachOfTwoDays_ChargesBoth()
    {
        DateTimeOffset[] passages = [PassageAt(7, 15), PassageOnNextDayAt(7, 15)];

        var result = await _service.Calculate(PassagesOf(VehicleType.Car, passages), CancellationToken.None);

        Assert.Equal(36, result.Value!.TotalFee);
        Assert.Equal(2, result.Value!.ChargedPassages.Length);
    }

    [Fact]
    public async Task Calculate_NullPassages_Throws()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _service.Calculate(null!, CancellationToken.None));
    }

    [Fact]
    public async Task Calculate_PassagesReportingDifferentVehicleTypes_ChargesEachOnItsOwnType()
    {
        TollPassage[] passages =
        [
            TollPassage.Create("ABC123", VehicleType.Car, PassageAt(7, 15)),
            TollPassage.Create("ABC123", VehicleType.Motorbike, PassageAt(9, 15)),
        ];

        var result = await _service.Calculate(passages, CancellationToken.None);

        Assert.Equal(18, result.Value!.TotalFee);
        Assert.Single(result.Value!.ChargedPassages);
    }

    private static TollPassage[] PassagesOf(VehicleType vehicleType, DateTimeOffset[] passedAt)
    {
        return Array.ConvertAll(passedAt, moment => TollPassage.Create("ABC123", vehicleType, moment));
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
