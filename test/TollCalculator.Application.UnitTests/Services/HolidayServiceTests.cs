using NSubstitute;
using TollCalculator.Application.Results;
using TollCalculator.Application.Services.Implementations;
using TollCalculator.Domain.Models;
using TollCalculator.Domain.Services;
using Xunit;

namespace TollCalculator.Application.UnitTests.Services;

public class HolidayServiceTests
{
    private readonly IHolidayProvider _holidayProvider = Substitute.For<IHolidayProvider>();
    private readonly HolidayService _service;

    public HolidayServiceTests()
    {
        _service = new HolidayService(_holidayProvider);
    }

    [Fact]
    public void GetHolidays_WithYear_ReturnsThatYearFromTheProvider()
    {
        var holidays = new[] { Holiday.Create(new DateOnly(2026, 12, 25), "Juldagen") };
        _holidayProvider.GetHolidays(2026).Returns(holidays);

        var result = _service.GetHolidays(2026);

        Assert.True(result.IsSuccess);
        Assert.Equal(holidays, result.Value);
    }

    [Fact]
    public void GetHolidays_WithoutYear_FallsBackToTheCurrentYear()
    {
        var currentYear = DateTime.UtcNow.Year;
        var holidays = new[] { Holiday.Create(new DateOnly(currentYear, 12, 25), "Juldagen") };
        _holidayProvider.GetHolidays(currentYear).Returns(holidays);

        var result = _service.GetHolidays(null);

        Assert.True(result.IsSuccess);
        Assert.Equal(holidays, result.Value);
    }

    [Fact]
    public void GetHolidays_AnyFutureYear_IsResolvedWithoutASeededTable()
    {
        _holidayProvider.GetHolidays(2099).Returns([Holiday.Create(new DateOnly(2099, 12, 25), "Juldagen")]);

        var result = _service.GetHolidays(2099);

        Assert.True(result.IsSuccess);
        Assert.Equal(new DateOnly(2099, 12, 25), Assert.Single(result.Value!).Date);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(10000)]
    public void GetHolidays_ImpossibleYear_ReturnsInvalidWithoutReading(int year)
    {
        var result = _service.GetHolidays(year);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationResultStatus.ValidationError, result.Status);

        _holidayProvider.DidNotReceiveWithAnyArgs().GetHolidays(0);
    }

    [Fact]
    public void Constructor_NullHolidayProvider_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new HolidayService(null!));
    }
}
