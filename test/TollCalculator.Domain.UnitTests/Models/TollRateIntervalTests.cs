using TollCalculator.Domain.Models;
using Xunit;

namespace TollCalculator.Domain.UnitTests.Models;

public class TollRateIntervalTests
{
    [Theory]
    [InlineData(6, 0)]
    [InlineData(6, 15)]
    [InlineData(6, 29)]
    public void Covers_TimeWithinInterval_ReturnsTrue(int hour, int minute)
    {
        var interval = TollRateInterval.Create(new TimeOnly(6, 0), new TimeOnly(6, 29), 8);

        Assert.True(interval.Covers(new TimeOnly(hour, minute)));
    }

    [Theory]
    [InlineData(5, 59)]
    [InlineData(6, 30)]
    public void Covers_TimeOutsideInterval_ReturnsFalse(int hour, int minute)
    {
        var interval = TollRateInterval.Create(new TimeOnly(6, 0), new TimeOnly(6, 29), 8);

        Assert.False(interval.Covers(new TimeOnly(hour, minute)));
    }

    [Fact]
    public void Create_EndBeforeStart_Throws()
    {
        Assert.Throws<ArgumentException>(() => TollRateInterval.Create(new TimeOnly(7, 0), new TimeOnly(6, 0), 8));
    }

    [Fact]
    public void Create_NegativeAmount_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => TollRateInterval.Create(new TimeOnly(6, 0), new TimeOnly(6, 29), -1));
    }
}
