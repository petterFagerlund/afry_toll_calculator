using TollCalculator.Domain.Models;

namespace TollCalculator.Domain.UnitTests.Models;

public class TollRateIntervalTests
{
    [Theory]
    [InlineData(6, 0, 0)]
    [InlineData(6, 15, 0)]
    [InlineData(6, 29, 0)]
    [InlineData(6, 29, 59)]
    public void Covers_TimeWithinInterval_ReturnsTrue(int hour, int minute, int second)
    {
        var interval = TollRateInterval.Create(new TimeOnly(6, 0), new TimeOnly(6, 30), 8);

        Assert.True(interval.Covers(new TimeOnly(hour, minute, second)));
    }

    [Theory]
    [InlineData(5, 59, 59)]
    [InlineData(6, 30, 0)]
    [InlineData(6, 30, 1)]
    public void Covers_TimeOutsideInterval_ReturnsFalse(int hour, int minute, int second)
    {
        var interval = TollRateInterval.Create(new TimeOnly(6, 0), new TimeOnly(6, 30), 8);

        Assert.False(interval.Covers(new TimeOnly(hour, minute, second)));
    }

    [Fact]
    public void Create_EndBeforeStart_Throws()
    {
        Assert.Throws<ArgumentException>(() => TollRateInterval.Create(new TimeOnly(7, 0), new TimeOnly(6, 0), 8));
    }

    [Fact]
    public void Create_EndEqualToStart_Throws()
    {
        Assert.Throws<ArgumentException>(() => TollRateInterval.Create(new TimeOnly(6, 0), new TimeOnly(6, 0), 8));
    }

    [Fact]
    public void Create_NegativeAmount_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => TollRateInterval.Create(new TimeOnly(6, 0), new TimeOnly(6, 30), -1));
    }
}
