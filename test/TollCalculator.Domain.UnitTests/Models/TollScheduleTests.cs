using TollCalculator.Domain.Models;

namespace TollCalculator.Domain.UnitTests.Models;

public class TollScheduleTests
{
    [Theory]
    [InlineData(0, 0, 0, 0)]
    [InlineData(5, 59, 59, 0)]
    [InlineData(6, 0, 0, 8)]
    [InlineData(6, 29, 0, 8)]
    [InlineData(6, 29, 30, 8)]
    [InlineData(6, 29, 59, 8)]
    [InlineData(6, 30, 0, 13)]
    [InlineData(6, 59, 59, 13)]
    [InlineData(7, 0, 0, 18)]
    [InlineData(7, 59, 59, 18)]
    [InlineData(8, 0, 0, 13)]
    [InlineData(8, 29, 59, 13)]
    [InlineData(8, 30, 0, 8)]
    [InlineData(9, 5, 0, 8)]
    [InlineData(12, 0, 0, 8)]
    [InlineData(14, 59, 59, 8)]
    [InlineData(15, 0, 0, 13)]
    [InlineData(15, 29, 59, 13)]
    [InlineData(15, 30, 0, 18)]
    [InlineData(16, 59, 59, 18)]
    [InlineData(17, 0, 0, 13)]
    [InlineData(17, 59, 59, 13)]
    [InlineData(18, 0, 0, 8)]
    [InlineData(18, 29, 59, 8)]
    [InlineData(18, 30, 0, 0)]
    [InlineData(23, 59, 59, 0)]
    public void FeeFor_TimeOfDay_ReturnsScheduledAmount(int hour, int minute, int second, int expected)
    {
        var schedule = CreateStandardSchedule();

        Assert.Equal(expected, schedule.FeeFor(new TimeOnly(hour, minute, second)));
    }

    [Fact]
    public void FeeFor_EveryTimeOfDayInTheChargedWindow_IsCoveredByAnInterval()
    {
        var schedule = CreateStandardSchedule();
        var timeOfDay = new TimeOnly(6, 0);
        var chargedWindowEnd = new TimeOnly(18, 30);

        while (timeOfDay < chargedWindowEnd)
        {
            Assert.NotEqual(0, schedule.FeeFor(timeOfDay));
            timeOfDay = timeOfDay.Add(TimeSpan.FromSeconds(1));
        }
    }

    [Fact]
    public void FeeFor_EmptySchedule_ReturnsZero()
    {
        var schedule = TollSchedule.Create([]);

        Assert.Equal(0, schedule.FeeFor(new TimeOnly(7, 0)));
    }

    [Fact]
    public void Create_OverlappingIntervals_Throws()
    {
        var first = TollRateInterval.Create(new TimeOnly(6, 0), new TimeOnly(7, 0), 8);
        var overlapping = TollRateInterval.Create(new TimeOnly(6, 30), new TimeOnly(7, 30), 13);

        Assert.Throws<ArgumentException>(() => TollSchedule.Create([first, overlapping]));
    }

    [Fact]
    public void Create_IntervalsThatMeetExactly_AreAccepted()
    {
        var first = TollRateInterval.Create(new TimeOnly(6, 0), new TimeOnly(6, 30), 8);
        var adjoining = TollRateInterval.Create(new TimeOnly(6, 30), new TimeOnly(7, 0), 13);

        var schedule = TollSchedule.Create([first, adjoining]);

        Assert.Equal(2, schedule.Intervals.Count);
    }

    [Fact]
    public void Create_UnorderedIntervals_AreOrderedByStart()
    {
        var later = TollRateInterval.Create(new TimeOnly(7, 0), new TimeOnly(8, 0), 18);
        var earlier = TollRateInterval.Create(new TimeOnly(6, 0), new TimeOnly(7, 0), 8);

        var schedule = TollSchedule.Create([later, earlier]);

        Assert.Equal(earlier, schedule.Intervals[0]);
        Assert.Equal(later, schedule.Intervals[1]);
    }

    private static TollSchedule CreateStandardSchedule()
    {
        return TollSchedule.Create(
        [
            TollRateInterval.Create(new TimeOnly(6, 0), new TimeOnly(6, 30), 8),
            TollRateInterval.Create(new TimeOnly(6, 30), new TimeOnly(7, 0), 13),
            TollRateInterval.Create(new TimeOnly(7, 0), new TimeOnly(8, 0), 18),
            TollRateInterval.Create(new TimeOnly(8, 0), new TimeOnly(8, 30), 13),
            TollRateInterval.Create(new TimeOnly(8, 30), new TimeOnly(15, 0), 8),
            TollRateInterval.Create(new TimeOnly(15, 0), new TimeOnly(15, 30), 13),
            TollRateInterval.Create(new TimeOnly(15, 30), new TimeOnly(17, 0), 18),
            TollRateInterval.Create(new TimeOnly(17, 0), new TimeOnly(18, 0), 13),
            TollRateInterval.Create(new TimeOnly(18, 0), new TimeOnly(18, 30), 8)
        ]);
    }
}
