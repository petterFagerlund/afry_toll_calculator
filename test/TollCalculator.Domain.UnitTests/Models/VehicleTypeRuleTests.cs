using TollCalculator.Domain.Enums;
using TollCalculator.Domain.Models;
using Xunit;

namespace TollCalculator.Domain.UnitTests.Models;

public class VehicleTypeRuleTests
{
    [Fact]
    public void Create_TollFreeType_SetsProperties()
    {
        var rule = VehicleTypeRule.Create(VehicleType.Motorbike, true);

        Assert.Equal(VehicleType.Motorbike, rule.VehicleType);
        Assert.True(rule.IsTollFree);
    }

    [Fact]
    public void Create_ChargeableType_SetsProperties()
    {
        var rule = VehicleTypeRule.Create(VehicleType.Car, false);

        Assert.False(rule.IsTollFree);
    }
}
