using TollCalculator.Domain.Models;
using TollCalculator.WebApi.Host.Mappers;
using Xunit;

namespace TollCalculator.WebApi.Host.IntegrationTests.Mappers;

public class HolidayMapperTests
{
    [Fact]
    public void ToResponses_MapsTheDateAndName()
    {
        var holiday = Holiday.Create(new DateOnly(2026, 1, 1), "Nyårsdagen");

        var response = Assert.Single(HolidayMapper.ToResponses([holiday]));

        Assert.Equal(new DateOnly(2026, 1, 1), response.Date);
        Assert.Equal("Nyårsdagen", response.Name);
    }

    [Fact]
    public void ToResponses_MapsEveryHoliday()
    {
        var newYear = Holiday.Create(new DateOnly(2026, 1, 1), "Nyårsdagen");
        var epiphany = Holiday.Create(new DateOnly(2026, 1, 6), "Trettondedag jul");

        var responses = HolidayMapper.ToResponses([newYear, epiphany]);

        Assert.Equal(["Nyårsdagen", "Trettondedag jul"], responses.Select(response => response.Name));
    }

    [Fact]
    public void ToResponses_MapsAnEmptySet()
    {
        Assert.Empty(HolidayMapper.ToResponses([]));
    }

    [Fact]
    public void ToResponses_ThrowsWhenTheHolidaysAreNull()
    {
        Assert.Throws<ArgumentNullException>(() => HolidayMapper.ToResponses(null!));
    }
}
