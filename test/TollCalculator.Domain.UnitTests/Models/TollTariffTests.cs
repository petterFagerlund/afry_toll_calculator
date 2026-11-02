using TollCalculator.Domain.Models;
using Xunit;

namespace TollCalculator.Domain.UnitTests.Models;

public class TollTariffTests
{
    [Fact]
    public void Create_ValidDailyCap_SetsProperties()
    {
        var schedule = TollSchedule.Create([]);

        var tariff = TollTariff.Create(schedule, 60);

        Assert.Same(schedule, tariff.Schedule);
        Assert.Equal(60, tariff.DailyCap);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_DailyCapBelowOne_Throws(int dailyCap)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => TollTariff.Create(TollSchedule.Create([]), dailyCap));
    }

    [Fact]
    public void Create_NullSchedule_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => TollTariff.Create(null!, 60));
    }
}
