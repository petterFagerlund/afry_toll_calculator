using NSubstitute;
using TollCalculator.Application.Services.Implementations;
using TollCalculator.Domain.Models;
using TollCalculator.Domain.Repositories;
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
        _tollScheduleRepository.GetRateIntervalsAsync(Arg.Any<CancellationToken>()).Returns([morning, rush]);

        var result = await _service.GetTollRatesAsync(CancellationToken.None);

        Assert.Equal([morning, rush], result);
    }

    [Fact]
    public async Task GetTollRatesAsync_EmptySchedule_ReturnsNoIntervals()
    {
        _tollScheduleRepository.GetRateIntervalsAsync(Arg.Any<CancellationToken>()).Returns([]);

        var result = await _service.GetTollRatesAsync(CancellationToken.None);

        Assert.Empty(result);
    }
}
