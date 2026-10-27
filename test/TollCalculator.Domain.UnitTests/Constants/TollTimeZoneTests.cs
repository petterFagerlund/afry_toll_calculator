using TollCalculator.Domain.Constants;
using Xunit;

namespace TollCalculator.Domain.UnitTests.Constants;

public class TollTimeZoneTests
{
    [Fact]
    public void DateOf_UsesTheTollTimeZoneRatherThanTheOffsetOfTheMoment()
    {
        var justBeforeLocalMidnight = new DateTimeOffset(2026, 3, 16, 23, 30, 0, TimeSpan.Zero);

        Assert.Equal(new DateOnly(2026, 3, 17), TollTimeZone.DateOf(justBeforeLocalMidnight));
    }

    [Fact]
    public void DateOf_KeepsTheDayForAMomentAlreadyInTheTollTimeZone()
    {
        var morning = new DateTimeOffset(2026, 3, 16, 7, 15, 0, TimeSpan.FromHours(1));

        Assert.Equal(new DateOnly(2026, 3, 16), TollTimeZone.DateOf(morning));
    }

    [Fact]
    public void DateOf_HandlesASummerTimeMoment()
    {
        var summerEvening = new DateTimeOffset(2026, 7, 1, 22, 30, 0, TimeSpan.Zero);

        Assert.Equal(new DateOnly(2026, 7, 2), TollTimeZone.DateOf(summerEvening));
    }
}
