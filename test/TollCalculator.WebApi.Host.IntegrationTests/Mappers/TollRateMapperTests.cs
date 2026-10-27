using TollCalculator.Domain.Models;
using TollCalculator.WebApi.Host.Mappers;
using Xunit;

namespace TollCalculator.WebApi.Host.IntegrationTests.Mappers;

public class TollRateMapperTests
{
    [Fact]
    public void ToResponses_FormatsTheIntervalBoundariesAsHoursAndMinutes()
    {
        var interval = TollRateInterval.Create(new TimeOnly(6, 0), new TimeOnly(6, 29), 8);

        var response = Assert.Single(TollRateMapper.ToResponses([interval]));

        Assert.Equal("06:00", response.Start);
        Assert.Equal("06:29", response.End);
        Assert.Equal(8, response.Amount);
    }

    [Fact]
    public void ToResponses_KeepsTheOrderOfTheSchedule()
    {
        var morning = TollRateInterval.Create(new TimeOnly(6, 30), new TimeOnly(6, 59), 13);
        var rush = TollRateInterval.Create(new TimeOnly(7, 0), new TimeOnly(7, 59), 18);
        var midday = TollRateInterval.Create(new TimeOnly(8, 30), new TimeOnly(14, 59), 8);

        var responses = TollRateMapper.ToResponses([morning, rush, midday]);

        Assert.Equal(["06:30", "07:00", "08:30"], responses.Select(response => response.Start));
        Assert.Equal(["06:59", "07:59", "14:59"], responses.Select(response => response.End));
    }

    [Fact]
    public void ToResponses_MapsAnEmptySchedule()
    {
        Assert.Empty(TollRateMapper.ToResponses([]));
    }

    [Fact]
    public void ToResponses_ThrowsWhenTheIntervalsAreNull()
    {
        Assert.Throws<ArgumentNullException>(() => TollRateMapper.ToResponses(null!));
    }
}
