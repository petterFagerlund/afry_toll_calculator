using NSubstitute;
using TollCalculator.Application.Providers;
using TollCalculator.Application.Results;
using TollCalculator.Domain.Models;
using TollCalculator.Application.Services;
using TollCalculator.Application.Services.Implementations;

namespace TollCalculator.Application.UnitTests.Services;

public class TollFreeDateServiceTests
{
    private readonly IHolidayProvider _holidayProvider = Substitute.For<IHolidayProvider>();
    private readonly ITollFreeDateService _service;

    public TollFreeDateServiceTests()
    {
        _holidayProvider.GetHolidays(2026).Returns(ApplicationResult<Holiday[]>.Success([
            Holiday.Create(new DateOnly(2026, 1, 1), "Nyårsdagen"),
            Holiday.Create(new DateOnly(2026, 6, 19), "Midsommarafton")
        ]));
        _holidayProvider.GetHolidays(2027).Returns(ApplicationResult<Holiday[]>.Success([
            Holiday.Create(new DateOnly(2027, 1, 1), "Nyårsdagen")
        ]));

        _service = new TollFreeDateService(_holidayProvider);
    }

    [Theory]
    [InlineData(2026, 10, 24)]
    [InlineData(2026, 10, 25)]
    [InlineData(2027, 10, 23)]
    [InlineData(2027, 10, 24)]
    public void IsTollFree_Weekend_ReturnsTrue(int year, int month, int day)
    {
        Assert.True(_service.IsTollFree(new DateOnly(year, month, day)).Value);
    }

    [Theory]
    [InlineData(2026, 1, 1)]
    [InlineData(2027, 1, 1)]
    public void IsTollFree_Holiday_ReturnsTrue(int year, int month, int day)
    {
        Assert.True(_service.IsTollFree(new DateOnly(year, month, day)).Value);
    }

    [Theory]
    [InlineData(2026, 10, 23)]
    [InlineData(2027, 10, 22)]
    public void IsTollFree_OrdinaryWeekday_ReturnsFalse(int year, int month, int day)
    {
        Assert.False(_service.IsTollFree(new DateOnly(year, month, day)).Value);
    }

    [Fact]
    public void IsTollFree_SameDayInAYearWithoutThatHoliday_ReturnsFalse()
    {
        Assert.True(_service.IsTollFree(new DateOnly(2026, 6, 19)).Value);
        Assert.False(_service.IsTollFree(new DateOnly(2027, 6, 18)).Value);
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
        Assert.Throws<ArgumentNullException>(() => new TollFreeDateService(null!));
    }
}
