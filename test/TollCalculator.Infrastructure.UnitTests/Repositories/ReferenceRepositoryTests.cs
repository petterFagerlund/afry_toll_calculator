using Microsoft.Extensions.Options;
using TollCalculator.Infrastructure.Configuration;
using TollCalculator.Infrastructure.Repositories.Implementations;

namespace TollCalculator.Infrastructure.UnitTests.Repositories;

public class ReferenceRepositoryTests : IDisposable
{
    private readonly RepositoryTestContext _fixture = new();

    [Fact]
    public async Task GetTariffAsync_ReturnsTheSeededSchedule()
    {
        var repository = ScheduleRepositoryWithCap(60);

        var tariff = await repository.GetTariffAsync(CancellationToken.None);

        Assert.Equal(9, tariff.Schedule.Intervals.Count);
        Assert.Equal(18, tariff.Schedule.FeeFor(new TimeOnly(7, 30)));
        Assert.Equal(8, tariff.Schedule.FeeFor(new TimeOnly(9, 5)));
    }

    [Fact]
    public async Task GetTariffAsync_TakesTheDailyCapFromConfiguration()
    {
        var repository = ScheduleRepositoryWithCap(45);

        var tariff = await repository.GetTariffAsync(CancellationToken.None);

        Assert.Equal(45, tariff.DailyCap);
    }

    public void Dispose()
    {
        _fixture.Dispose();
    }

    private TollScheduleRepository ScheduleRepositoryWithCap(int dailyCap)
    {
        return new TollScheduleRepository(
            _fixture.Context, Options.Create(new TollOptions { DailyCap = dailyCap }));
    }
}
