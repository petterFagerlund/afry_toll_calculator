using TollCalculator.Domain.Models;
using TollCalculator.WebApi.Host.Mappers.Implementations;
using Xunit;

namespace TollCalculator.WebApi.Host.IntegrationTests.Mappers;

public class DailyTollMapperTests
{
    private static readonly DateTimeOffset Morning = new(2026, 3, 16, 7, 15, 0, TimeSpan.FromHours(1));
    private static readonly DateTimeOffset Afternoon = new(2026, 3, 16, 15, 40, 0, TimeSpan.FromHours(1));

    private readonly DailyTollMapper _mapper = new();

    [Fact]
    public void ToDailyTollResponse_MapsTheTotalAndEveryChargedPassage()
    {
        var dailyToll = new DailyToll(36, [new ChargedTollPassage(Morning, 18), new ChargedTollPassage(Afternoon, 18)]);

        var response = _mapper.ToDailyTollResponse("ABC123", new DateOnly(2026, 3, 16), dailyToll);

        Assert.Equal("ABC123", response.RegistrationNumber);
        Assert.Equal(new DateOnly(2026, 3, 16), response.Date);
        Assert.Equal(36, response.TotalFee);
        Assert.Equal([Morning, Afternoon], response.ChargedPassages.Select(passage => passage.PassedAt));
        Assert.Equal([18, 18], response.ChargedPassages.Select(passage => passage.Amount));
    }

    [Fact]
    public void ToDailyTollResponse_MapsADayWithoutChargedPassages()
    {
        var response = _mapper.ToDailyTollResponse("ABC123", new DateOnly(2026, 3, 15), DailyToll.None());

        Assert.Equal(0, response.TotalFee);
        Assert.Empty(response.ChargedPassages);
    }

    [Fact]
    public void ToCalculateTollResponse_MapsTheVehicleTypeAsAString()
    {
        var dailyToll = new DailyToll(18, [new ChargedTollPassage(Morning, 18)]);

        var response = _mapper.ToCalculateTollResponse("Car", dailyToll);

        Assert.Equal("Car", response.VehicleType);
        Assert.Equal(18, response.TotalFee);
        Assert.Equal(Morning, Assert.Single(response.ChargedPassages).PassedAt);
    }

    [Fact]
    public void ToCalculateTollResponse_MapsATollFreeResult()
    {
        var response = _mapper.ToCalculateTollResponse("Motorbike", DailyToll.None());

        Assert.Equal(0, response.TotalFee);
        Assert.Empty(response.ChargedPassages);
    }

    [Fact]
    public void ToDailyTollResponse_ThrowsWhenTheDailyTollIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => _mapper.ToDailyTollResponse("ABC123", new DateOnly(2026, 3, 16), null!));
    }

    [Fact]
    public void ToCalculateTollResponse_ThrowsWhenTheDailyTollIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => _mapper.ToCalculateTollResponse("Car", null!));
    }
}
