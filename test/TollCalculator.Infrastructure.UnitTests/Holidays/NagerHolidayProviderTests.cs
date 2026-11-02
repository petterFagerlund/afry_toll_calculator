using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using TollCalculator.Domain.Services;
using TollCalculator.Infrastructure.Holidays;
using Xunit;

namespace TollCalculator.Infrastructure.UnitTests.Holidays;

public class NagerHolidayProviderTests
{
    private readonly IHolidayProvider _provider =
        new NagerHolidayProvider(NullLogger<NagerHolidayProvider>.Instance);

    [Theory]
    [InlineData(2026, 1, 1, "Nyårsdagen")]
    [InlineData(2026, 1, 6, "Trettondedag jul")]
    [InlineData(2026, 4, 3, "Långfredagen")]
    [InlineData(2026, 4, 6, "Annandag påsk")]
    [InlineData(2026, 5, 1, "Första maj")]
    [InlineData(2026, 5, 14, "Kristi himmelsfärds dag")]
    [InlineData(2026, 6, 6, "Sveriges nationaldag")]
    [InlineData(2026, 12, 25, "Juldagen")]
    [InlineData(2026, 12, 26, "Annandag jul")]
    public void GetHolidays_ReturnsTheKnownSwedishHolidays(int year, int month, int day, string name)
    {
        var holidays = _provider.GetHolidays(year);

        var holiday = Assert.Single(holidays, candidate => candidate.Date == new DateOnly(year, month, day));
        Assert.Equal(name, holiday.Name);
    }

    [Theory]
    [InlineData(2030, 4, 19)]
    [InlineData(2040, 3, 30)]
    [InlineData(2099, 4, 10)]
    public void GetHolidays_ResolvesEasterForYearsBeyondAnySeededTable(int year, int month, int day)
    {
        var holidays = _provider.GetHolidays(year);

        Assert.Contains(holidays, holiday => holiday.Date == new DateOnly(year, month, day));
    }

    [Fact]
    public void GetHolidays_IncludesTheEveDaysNagerReportsAsPublic()
    {
        var holidays = _provider.GetHolidays(2026);

        Assert.Contains(holidays, holiday => holiday.Date == new DateOnly(2026, 6, 19));
        Assert.Contains(holidays, holiday => holiday.Date == new DateOnly(2026, 12, 24));
        Assert.Contains(holidays, holiday => holiday.Date == new DateOnly(2026, 12, 31));
    }

    [Fact]
    public void GetHolidays_ReturnsThemInDateOrder()
    {
        var holidays = _provider.GetHolidays(2026);

        for (var i = 1; i < holidays.Length; i++)
        {
            Assert.True(holidays[i - 1].Date < holidays[i].Date);
        }
    }

    [Fact]
    public void GetHolidays_ReturnsOnlyDatesWithinTheRequestedYear()
    {
        var holidays = _provider.GetHolidays(2026);

        Assert.All(holidays, holiday => Assert.Equal(2026, holiday.Date.Year));
    }

    [Fact]
    public void GetHolidays_CalledTwice_ReturnsTheSameCachedInstance()
    {
        Assert.Same(_provider.GetHolidays(2026), _provider.GetHolidays(2026));
    }
}
