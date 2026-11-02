using NSubstitute;
using TollCalculator.Domain.Models;
using TollCalculator.Domain.Services;
using TollCalculator.Domain.Services.Implementations;
using Xunit;

namespace TollCalculator.Domain.UnitTests.Services;

public class WeekendAndHolidayServiceTests
{
    private readonly IHolidayProvider _holidayProvider = Substitute.For<IHolidayProvider>();
    private readonly ITollFreeDateService _service;

    public WeekendAndHolidayServiceTests()
    {
        _holidayProvider.GetHolidays(2026).Returns([
            Holiday.Create(new DateOnly(2026, 1, 1), "Nyårsdagen"),
            Holiday.Create(new DateOnly(2026, 6, 19), "Midsommarafton")
        ]);
        _holidayProvider.GetHolidays(2027).Returns([
            Holiday.Create(new DateOnly(2027, 1, 1), "Nyårsdagen")
        ]);

        _service = new WeekendAndHolidayService(_holidayProvider);
    }

    [Theory]
    [InlineData(2026, 10, 24)]
    [InlineData(2026, 10, 25)]
    [InlineData(2027, 10, 23)]
    [InlineData(2027, 10, 24)]
    public void IsTollFree_Weekend_ReturnsTrue(int year, int month, int day)
    {
        Assert.True(_service.IsTollFree(new DateOnly(year, month, day)));
    }

    [Theory]
    [InlineData(2026, 1, 1)]
    [InlineData(2027, 1, 1)]
    public void IsTollFree_Holiday_ReturnsTrue(int year, int month, int day)
    {
        Assert.True(_service.IsTollFree(new DateOnly(year, month, day)));
    }

    [Theory]
    [InlineData(2026, 10, 23)]
    [InlineData(2027, 10, 22)]
    public void IsTollFree_OrdinaryWeekday_ReturnsFalse(int year, int month, int day)
    {
        Assert.False(_service.IsTollFree(new DateOnly(year, month, day)));
    }

    [Fact]
    public void IsTollFree_SameDayInAYearWithoutThatHoliday_ReturnsFalse()
    {
        Assert.True(_service.IsTollFree(new DateOnly(2026, 6, 19)));
        Assert.False(_service.IsTollFree(new DateOnly(2027, 6, 18)));
    }

    [Fact]
    public void IsTollFree_Weekend_DoesNotAskForTheYearsHolidays()
    {
        _service.IsTollFree(new DateOnly(2026, 10, 24));

        _holidayProvider.DidNotReceiveWithAnyArgs().GetHolidays(0);
    }

    [Fact]
    public void IsTollFree_AsksOnlyForTheYearOfTheDate()
    {
        _service.IsTollFree(new DateOnly(2026, 10, 23));

        _holidayProvider.Received(1).GetHolidays(2026);
        _holidayProvider.DidNotReceive().GetHolidays(2027);
    }

    [Fact]
    public void Constructor_NullHolidayProvider_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new WeekendAndHolidayService(null!));
    }
}
