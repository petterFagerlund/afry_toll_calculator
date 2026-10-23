using TollCalculator.Domain.Constants;
using TollCalculator.Domain.Enums;
using TollCalculator.Domain.Models;
using Xunit;

namespace TollCalculator.Domain.UnitTests.Models;

public class VehicleTests
{
    [Theory]
    [InlineData("abc123", "ABC123")]
    [InlineData("  abc123  ", "ABC123")]
    [InlineData("AbC 123", "ABC 123")]
    public void Create_RegistrationNumberNeedsNormalising_TrimsAndUppercases(string input, string expected)
    {
        var vehicle = Vehicle.Create(input, VehicleType.Car);

        Assert.Equal(expected, vehicle.RegistrationNumber);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_MissingRegistrationNumber_Throws(string? registrationNumber)
    {
        Assert.Throws<ArgumentException>(() => Vehicle.Create(registrationNumber!, VehicleType.Car));
    }

    [Fact]
    public void Create_SameRegistrationNumberDifferentCasing_VehiclesAreEqual()
    {
        var first = Vehicle.Create("abc123", VehicleType.Car);
        var second = Vehicle.Create("ABC123", VehicleType.Car);

        Assert.Equal(first, second);
    }

    [Fact]
    public void Create_SameRegistrationNumberDifferentType_VehiclesAreNotEqual()
    {
        var car = Vehicle.Create("ABC123", VehicleType.Car);
        var tractor = Vehicle.Create("ABC123", VehicleType.Tractor);

        Assert.NotEqual(car, tractor);
    }

    [Theory]
    [InlineData(VehicleType.Motorbike)]
    [InlineData(VehicleType.Tractor)]
    [InlineData(VehicleType.Emergency)]
    [InlineData(VehicleType.Diplomat)]
    [InlineData(VehicleType.Foreign)]
    [InlineData(VehicleType.Military)]
    public void IsTollFree_TollFreeVehicleType_ReturnsTrue(VehicleType vehicleType)
    {
        var vehicle = Vehicle.Create("ABC123", vehicleType);

        Assert.True(vehicle.IsTollFree);
    }

    [Theory]
    [InlineData(VehicleType.Car)]
    [InlineData(VehicleType.Unknown)]
    public void IsTollFree_ChargeableVehicleType_ReturnsFalse(VehicleType vehicleType)
    {
        var vehicle = Vehicle.Create("ABC123", vehicleType);

        Assert.False(vehicle.IsTollFree);
    }

    [Fact]
    public void All_TollFreeVehicleTypes_ContainsTheSixFreeTypes()
    {
        Assert.Equal(6, TollFreeVehicleTypes.All.Length);
    }
}
