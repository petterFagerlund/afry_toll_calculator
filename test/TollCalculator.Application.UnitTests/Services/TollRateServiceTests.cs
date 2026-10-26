using NSubstitute;
using TollCalculator.Application.Services.Implementations;
using TollCalculator.Domain.Models;
using TollCalculator.Infrastructure.Repositories;
using Xunit;

namespace TollCalculator.Application.UnitTests.Services;

public class TollRateServiceTests
{
    private readonly ITollScheduleRepository _tollScheduleRepository = Substitute.For<ITollScheduleRepository>();
    private readonly TollRateService _service;

    public TollRateServiceTests()
    {
        _service = new TollRateService(_tollScheduleRepository);
    }

    [Fact]
    public async Task GetTollRatesAsync_ReturnsTheIntervalsFromTheSchedule()
    {
        var morning = TollRateInterval.Create(new TimeOnly(6, 0), new TimeOnly(6, 29), 8);
        var rush = TollRateInterval.Create(new TimeOnly(7, 0), new TimeOnly(7, 59), 18);
        _tollScheduleRepository.GetScheduleAsync(Arg.Any<CancellationToken>())
            .Returns(TollSchedule.Create([morning, rush]));

        var result = await _service.GetTollRatesAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal([morning, rush], result.Value!);
    }

    [Fact]
    public async Task GetTollRatesAsync_EmptySchedule_ReturnsNoIntervals()
    {
        _tollScheduleRepository.GetScheduleAsync(Arg.Any<CancellationToken>()).Returns(TollSchedule.Create([]));

        var result = await _service.GetTollRatesAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }
}
