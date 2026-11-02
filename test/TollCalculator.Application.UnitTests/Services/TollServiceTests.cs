using NSubstitute;
using TollCalculator.Application.Providers;
using TollCalculator.Application.Repositories;
using TollCalculator.Application.Results;
using TollCalculator.Application.Services.Implementations;
using TollCalculator.Domain.Constants;
using TollCalculator.Domain.Enums;
using TollCalculator.Domain.Models;

namespace TollCalculator.Application.UnitTests.Services;

public class TollServiceTests
{
    private static readonly DateOnly Monday = new(2026, 3, 16);

    private readonly ITollPassageRepository _tollPassageRepository = Substitute.For<ITollPassageRepository>();
    private readonly ITollScheduleRepository _tollScheduleRepository = Substitute.For<ITollScheduleRepository>();
    private readonly IHolidayProvider _holidayProvider = Substitute.For<IHolidayProvider>();
    private readonly TollService _service;

    public TollServiceTests()
    {
        _tollScheduleRepository.GetTariffAsync(Arg.Any<CancellationToken>())
            .Returns(TollTariff.Create(TestSchedules.Standard(), 60));

        _holidayProvider.GetHolidays(Arg.Any<int>()).Returns(ApplicationResult<Holiday[]>.Success([]));

        _service = new TollService(
            _tollPassageRepository,
            new DailyTollService(
                new TollFeeService(new TollFreeDateService(_holidayProvider)),
                _tollScheduleRepository));
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
