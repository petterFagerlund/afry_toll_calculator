using TollCalculator.Domain.Models;
using TollCalculator.Domain.Services;
using TollCalculator.Domain.Services.Implementations;
using Xunit;

namespace TollCalculator.Domain.UnitTests.Services;

public class WeekendAndHolidayServiceTests
{
    [Theory]
    [InlineData(2026, 10, 24)]
    [InlineData(2026, 10, 25)]
    [InlineData(2027, 10, 23)]
    [InlineData(2027, 10, 24)]
    public void IsTollFree_Weekend_ReturnsTrue(int year, int month, int day)
    {
        var service = CreateService();

        Assert.True(service.IsTollFree(new DateOnly(year, month, day)));
    }

    [Theory]
    [InlineData(2026, 1, 1)]
    [InlineData(2027, 1, 1)]
    public void IsTollFree_Holiday_ReturnsTrue(int year, int month, int day)
    {
        var service = CreateService();

        Assert.True(service.IsTollFree(new DateOnly(year, month, day)));
    }

    [Theory]
    [InlineData(2026, 10, 23)]
    [InlineData(2027, 10, 22)]
    public void IsTollFree_OrdinaryWeekday_ReturnsFalse(int year, int month, int day)
    {
        var service = CreateService();

        Assert.False(service.IsTollFree(new DateOnly(year, month, day)));
    }

    [Fact]
    public void IsTollFree_SameDayInAYearWithoutThatHoliday_ReturnsFalse()
    {
        var service = CreateService();

        Assert.True(service.IsTollFree(new DateOnly(2026, 6, 19)));
        Assert.False(service.IsTollFree(new DateOnly(2027, 6, 18)));
    }

    [Fact]
    public void IsTollFree_NoHolidaysConfigured_WeekdayIsCharged()
    {
        var service = new WeekendAndHolidayService([]);

        Assert.False(service.IsTollFree(new DateOnly(2026, 10, 23)));
    }

    private static ITollFreeDateService CreateService()
    {
        return new WeekendAndHolidayService(
        [
            Holiday.Create(new DateOnly(2026, 1, 1), "Nyårsdagen"),
            Holiday.Create(new DateOnly(2026, 6, 19), "Midsommarafton"),
            Holiday.Create(new DateOnly(2027, 1, 1), "Nyårsdagen")
        ]);
    }
}
