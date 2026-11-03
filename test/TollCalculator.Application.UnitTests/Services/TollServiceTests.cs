using NSubstitute;
using TollCalculator.Application.Providers;
using TollCalculator.Application.Repositories;
using TollCalculator.Application.Results;
using TollCalculator.Application.Services;
using TollCalculator.Application.Services.Implementations;
using TollCalculator.Domain.Constants;
using TollCalculator.Domain.Enums;
using TollCalculator.Domain.Models;

namespace TollCalculator.Application.UnitTests.Services;

public class TollServiceTests
{
    private const int DailyCap = 60;
    private static readonly DateOnly Monday = new(2026, 3, 16);
    private static readonly DateOnly NewYearsDay = new(2026, 1, 1);

    private readonly ITollPassageRepository _tollPassageRepository = Substitute.For<ITollPassageRepository>();
    private readonly ITollScheduleRepository _tollScheduleRepository = Substitute.For<ITollScheduleRepository>();
    private readonly IHolidayProvider _holidayProvider = Substitute.For<IHolidayProvider>();
    private readonly ITollService _service;

    public TollServiceTests()
    {
        _tollScheduleRepository.GetTariffAsync(Arg.Any<CancellationToken>())
            .Returns(TollTariff.Create(TestSchedules.Standard(), DailyCap));

        _holidayProvider.GetHolidays(Arg.Any<int>())
            .Returns(ApplicationResult<Holiday[]>.Success([Holiday.Create(NewYearsDay, "Nyårsdagen")]));

        _service = new TollService(
            _tollPassageRepository,
            _tollScheduleRepository,
            new TollFeeService(new TollFreeDateService(_holidayProvider)));
    }

    [Fact]
    public async Task GetDailyTollAsync_DayWithPassages_ReturnsTheTotalAndBreakdown()
    {
        PassagesOn(Monday, VehicleType.Car, At(Monday, 7, 15), At(Monday, 15, 10));

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
        PassagesOn(Monday, VehicleType.Car);

        var result = await _service.GetDailyTollAsync("ABC123", Monday, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value!.TotalFee);
        Assert.Empty(result.Value.ChargedPassages);
    }

    [Fact]
    public async Task GetDailyTollAsync_ARegistrationNumberWithNoPassages_ReturnsZeroRatherThanNotFound()
    {
        _tollPassageRepository.GetForDayAsync("XYZ789", Monday, Arg.Any<CancellationToken>()).Returns([]);

        var result = await _service.GetDailyTollAsync("XYZ789", Monday, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value!.TotalFee);
    }

    [Fact]
    public async Task GetDailyTollAsync_DayWithNoPassages_DoesNotLoadTheTariff()
    {
        PassagesOn(Monday, VehicleType.Car);

        await _service.GetDailyTollAsync("ABC123", Monday, CancellationToken.None);

        await _tollScheduleRepository.DidNotReceive().GetTariffAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetDailyTollAsync_APassageReportingAnUnknownVehicleType_IsCharged()
    {
        PassagesOn(Monday, VehicleType.Unknown, At(Monday, 7, 15));

        var result = await _service.GetDailyTollAsync("ABC123", Monday, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(18, result.Value!.TotalFee);
    }

    [Fact]
    public async Task GetDailyTollAsync_APassageReportingATollFreeVehicleType_IsNotCharged()
    {
        PassagesOn(Monday, VehicleType.Motorbike, At(Monday, 7, 15));

        var result = await _service.GetDailyTollAsync("ABC123", Monday, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value!.TotalFee);
        Assert.Empty(result.Value.ChargedPassages);
    }

    [Fact]
    public async Task GetDailyTollAsync_UnsortedPassages_ChargesTheSameAsSorted()
    {
        PassagesOn(Monday, VehicleType.Car, At(Monday, 15, 35), At(Monday, 7, 15));

        var result = await _service.GetDailyTollAsync("ABC123", Monday, CancellationToken.None);

        Assert.Equal(36, result.Value!.TotalFee);
        Assert.Equal(At(Monday, 7, 15), result.Value.ChargedPassages[0].PassedAt);
    }

    [Fact]
    public async Task GetDailyTollAsync_TwoPassagesWithinTheSameHour_ChargesOnlyTheHighest()
    {
        PassagesOn(Monday, VehicleType.Car, At(Monday, 6, 15), At(Monday, 6, 35));

        var result = await _service.GetDailyTollAsync("ABC123", Monday, CancellationToken.None);

        Assert.Equal(13, result.Value!.TotalFee);
        Assert.Single(result.Value.ChargedPassages);
        Assert.Equal(At(Monday, 6, 35), result.Value.ChargedPassages[0].PassedAt);
    }

    [Fact]
    public async Task GetDailyTollAsync_TwoPassagesMoreThanAnHourApart_ChargesBoth()
    {
        PassagesOn(Monday, VehicleType.Car, At(Monday, 6, 15), At(Monday, 7, 25));

        var result = await _service.GetDailyTollAsync("ABC123", Monday, CancellationToken.None);

        Assert.Equal(26, result.Value!.TotalFee);
        Assert.Equal(2, result.Value.ChargedPassages.Length);
    }

    [Fact]
    public async Task GetDailyTollAsync_PassagesExactlySixtyMinutesApart_ChargesBoth()
    {
        PassagesOn(Monday, VehicleType.Car, At(Monday, 6, 0), At(Monday, 7, 0));

        var result = await _service.GetDailyTollAsync("ABC123", Monday, CancellationToken.None);

        Assert.Equal(26, result.Value!.TotalFee);
        Assert.Equal(2, result.Value.ChargedPassages.Length);
    }

    [Fact]
    public async Task GetDailyTollAsync_DayExceedingTheCap_StopsAtTheCap()
    {
        PassagesOn(
            Monday,
            VehicleType.Car,
            At(Monday, 6, 0),
            At(Monday, 7, 5),
            At(Monday, 8, 10),
            At(Monday, 9, 15),
            At(Monday, 15, 35));

        var result = await _service.GetDailyTollAsync("ABC123", Monday, CancellationToken.None);

        Assert.Equal(DailyCap, result.Value!.TotalFee);
        Assert.Equal(DailyCap, result.Value.ChargedPassages.Sum(charged => charged.Amount));
    }

    [Fact]
    public async Task GetDailyTollAsync_Holiday_ReturnsZero()
    {
        PassagesOn(NewYearsDay, VehicleType.Car, At(NewYearsDay, 7, 15));

        var result = await _service.GetDailyTollAsync("ABC123", NewYearsDay, CancellationToken.None);

        Assert.Equal(0, result.Value!.TotalFee);
        Assert.Empty(result.Value.ChargedPassages);
    }

    [Fact]
    public async Task GetDailyTollAsync_PassagesOutsideChargedHours_AreNotCharged()
    {
        PassagesOn(Monday, VehicleType.Car, At(Monday, 3, 0), At(Monday, 22, 0));

        var result = await _service.GetDailyTollAsync("ABC123", Monday, CancellationToken.None);

        Assert.Equal(0, result.Value!.TotalFee);
        Assert.Empty(result.Value.ChargedPassages);
    }

    [Fact]
    public async Task GetDailyTollAsync_FreePassageBeforeACharged_DoesNotOpenTheWindow()
    {
        PassagesOn(Monday, VehicleType.Car, At(Monday, 5, 30), At(Monday, 6, 15), At(Monday, 6, 45));

        var result = await _service.GetDailyTollAsync("ABC123", Monday, CancellationToken.None);

        Assert.Equal(13, result.Value!.TotalFee);
        Assert.Single(result.Value.ChargedPassages);
    }

    [Fact]
    public async Task GetDailyTollAsync_PassagesReportingDifferentVehicleTypes_ChargesEachOnItsOwnType()
    {
        TollPassage[] passages =
        [
            TollPassage.Create("ABC123", VehicleType.Car, At(Monday, 7, 15)),
            TollPassage.Create("ABC123", VehicleType.Motorbike, At(Monday, 9, 15)),
        ];
        _tollPassageRepository.GetForDayAsync("ABC123", Monday, Arg.Any<CancellationToken>()).Returns(passages);

        var result = await _service.GetDailyTollAsync("ABC123", Monday, CancellationToken.None);

        Assert.Equal(18, result.Value!.TotalFee);
        Assert.Single(result.Value.ChargedPassages);
    }

    [Fact]
    public async Task GetDailyTollAsync_HolidaysCannotBeResolved_ReturnsTheError()
    {
        _holidayProvider.GetHolidays(Arg.Any<int>()).Returns(ApplicationResult<Holiday[]>.Error("no holidays"));
        PassagesOn(Monday, VehicleType.Car, At(Monday, 7, 15));

        var result = await _service.GetDailyTollAsync("ABC123", Monday, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("no holidays", result.ErrorMessage);
    }

    private void PassagesOn(DateOnly date, VehicleType vehicleType, params DateTimeOffset[] timestamps)
    {
        var passages = Array.ConvertAll(
            timestamps, timestamp => TollPassage.Create("ABC123", vehicleType, timestamp));

        _tollPassageRepository.GetForDayAsync("ABC123", date, Arg.Any<CancellationToken>()).Returns(passages);
    }

    private static DateTimeOffset At(DateOnly date, int hour, int minute)
    {
        var local = date.ToDateTime(new TimeOnly(hour, minute));

        return new DateTimeOffset(local, TollTimeZone.Instance.GetUtcOffset(local));
    }
}
