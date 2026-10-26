using NSubstitute;
using TollCalculator.Application.Services.Implementations;
using TollCalculator.Domain.Models;
using TollCalculator.Infrastructure.Repositories;
using Xunit;

namespace TollCalculator.Application.UnitTests.Services;

public class HolidayServiceTests
{
    private readonly IHolidayRepository _holidayRepository = Substitute.For<IHolidayRepository>();
    private readonly HolidayService _service;

    public HolidayServiceTests()
    {
        _service = new HolidayService(_holidayRepository);
    }

    [Fact]
    public async Task GetHolidaysAsync_WithYear_ReturnsThatYearFromTheRepository()
    {
        var holidays = new[] { Holiday.Create(new DateOnly(2026, 12, 25), "Juldagen") };
        _holidayRepository.GetByYearAsync(2026, Arg.Any<CancellationToken>()).Returns(holidays);

        var result = await _service.GetHolidaysAsync(2026, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(holidays, result.Value);

        await _holidayRepository.DidNotReceiveWithAnyArgs().GetAllAsync(CancellationToken.None);
    }

    [Fact]
    public async Task GetHolidaysAsync_WithoutYear_ReturnsEveryHolidayFromTheRepository()
    {
        var holidays = new[]
        {
            Holiday.Create(new DateOnly(2026, 12, 25), "Juldagen"),
            Holiday.Create(new DateOnly(2027, 12, 25), "Juldagen")
        };
        _holidayRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(holidays);

        var result = await _service.GetHolidaysAsync(null, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(holidays, result.Value);

        await _holidayRepository.DidNotReceiveWithAnyArgs().GetByYearAsync(0, CancellationToken.None);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(10000)]
    public async Task GetHolidaysAsync_ImpossibleYear_ReturnsInvalidWithoutReading(int year)
    {
        var result = await _service.GetHolidaysAsync(year, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsInvalid);

        await _holidayRepository.DidNotReceiveWithAnyArgs().GetByYearAsync(0, CancellationToken.None);
        await _holidayRepository.DidNotReceiveWithAnyArgs().GetAllAsync(CancellationToken.None);
    }
}
