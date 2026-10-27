using TollCalculator.Domain.Enums;
using TollCalculator.Domain.Models;
using TollCalculator.WebApi.Host.Mappers.Implementations;
using Xunit;

namespace TollCalculator.WebApi.Host.IntegrationTests.Mappers;

public class VehicleTypeMapperTests
{
    private readonly VehicleTypeMapper _mapper = new();

    [Theory]
    [InlineData(VehicleType.Car, false)]
    [InlineData(VehicleType.Motorbike, true)]
    [InlineData(VehicleType.Military, true)]
    public void ToResponses_MapsTheVehicleTypeToItsNameAndTollFreeFlag(VehicleType vehicleType, bool isTollFree)
    {
        var rule = VehicleTypeRule.Create(vehicleType, isTollFree);

        var response = Assert.Single(_mapper.ToResponses([rule]));

        Assert.Equal(vehicleType.ToString(), response.VehicleType);
        Assert.Equal(isTollFree, response.IsTollFree);
    }

    [Fact]
    public void ToResponses_MapsEveryRule()
    {
        var car = VehicleTypeRule.Create(VehicleType.Car, isTollFree: false);
        var tractor = VehicleTypeRule.Create(VehicleType.Tractor, isTollFree: true);

        var responses = _mapper.ToResponses([car, tractor]);

        Assert.Equal(["Car", "Tractor"], responses.Select(response => response.VehicleType));
    }

    [Fact]
    public void ToResponses_MapsAnEmptySet()
    {
        Assert.Empty(_mapper.ToResponses([]));
    }

    [Fact]
    public void ToResponses_ThrowsWhenTheRulesAreNull()
    {
        Assert.Throws<ArgumentNullException>(() => _mapper.ToResponses(null!));
    }
}
