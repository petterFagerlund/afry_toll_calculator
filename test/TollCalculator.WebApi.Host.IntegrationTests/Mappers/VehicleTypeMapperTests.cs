using TollCalculator.Domain.Enums;
using TollCalculator.Domain.Models;
using TollCalculator.WebApi.Host.Mappers;
using Xunit;
using ContractVehicleType = TollCalculator.WebApi.Models.V1.VehicleType;

namespace TollCalculator.WebApi.Host.IntegrationTests.Mappers;

public class VehicleTypeMapperTests
{
    [Theory]
    [InlineData(VehicleType.Car, false)]
    [InlineData(VehicleType.Motorbike, true)]
    [InlineData(VehicleType.Military, true)]
    public void ToResponses_MapsTheVehicleTypeAndTollFreeFlag(VehicleType vehicleType, bool isTollFree)
    {
        var rule = VehicleTypeRule.Create(vehicleType, isTollFree);

        var response = Assert.Single(VehicleTypeMapper.ToResponses([rule]));

        Assert.Equal(vehicleType.ToString(), response.VehicleType.ToString());
        Assert.Equal(isTollFree, response.IsTollFree);
    }

    [Fact]
    public void ToResponses_MapsEveryRule()
    {
        var car = VehicleTypeRule.Create(VehicleType.Car, isTollFree: false);
        var tractor = VehicleTypeRule.Create(VehicleType.Tractor, isTollFree: true);

        var responses = VehicleTypeMapper.ToResponses([car, tractor]);

        Assert.Equal(
            [ContractVehicleType.Car, ContractVehicleType.Tractor],
            responses.Select(response => response.VehicleType));
    }

    [Fact]
    public void ToResponses_MapsAnEmptySet()
    {
        Assert.Empty(VehicleTypeMapper.ToResponses([]));
    }

    [Fact]
    public void ToResponses_ThrowsWhenTheRulesAreNull()
    {
        Assert.Throws<ArgumentNullException>(() => VehicleTypeMapper.ToResponses(null!));
    }

    [Fact]
    public void ToDomain_MapsEveryContractValueToTheDomainValueOfTheSameName()
    {
        foreach (var contractVehicleType in Enum.GetValues<ContractVehicleType>())
        {
            var domainVehicleType = VehicleTypeMapper.ToDomain(contractVehicleType);

            Assert.Equal(contractVehicleType.ToString(), domainVehicleType.ToString());
            Assert.NotEqual(VehicleType.Unknown, domainVehicleType);
        }
    }

    [Fact]
    public void ToContract_MapsEveryChargeableDomainValueToTheContractValueOfTheSameName()
    {
        foreach (var domainVehicleType in Enum.GetValues<VehicleType>())
        {
            if (domainVehicleType == VehicleType.Unknown)
            {
                continue;
            }

            Assert.Equal(domainVehicleType.ToString(), VehicleTypeMapper.ToContract(domainVehicleType).ToString());
        }
    }

    [Fact]
    public void TheTwoEnums_DeclareTheSameNames()
    {
        var contractNames = Enum.GetNames<ContractVehicleType>().Order();
        var domainNames = Enum.GetNames<VehicleType>().Where(name => name != nameof(VehicleType.Unknown)).Order();

        Assert.Equal(domainNames, contractNames);
    }

    [Fact]
    public void ToContract_UnknownDomainValue_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => VehicleTypeMapper.ToContract(VehicleType.Unknown));
    }
}
