using NSubstitute;
using TollCalculator.Application.Services.Implementations;
using TollCalculator.Domain.Enums;
using TollCalculator.Domain.Models;
using TollCalculator.Domain.Repositories;
using Xunit;

namespace TollCalculator.Application.UnitTests.Services;

public class VehicleTypeServiceTests
{
    private readonly IVehicleTypeRuleRepository _vehicleTypeRuleRepository =
        Substitute.For<IVehicleTypeRuleRepository>();

    private readonly VehicleTypeService _service;

    public VehicleTypeServiceTests()
    {
        _service = new VehicleTypeService(_vehicleTypeRuleRepository);
    }

    [Fact]
    public async Task GetVehicleTypeRulesAsync_ReturnsEveryRuleFromTheRepository()
    {
        var rules = new[]
        {
            VehicleTypeRule.Create(VehicleType.Car, false),
            VehicleTypeRule.Create(VehicleType.Emergency, true)
        };
        _vehicleTypeRuleRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(rules);

        var result = await _service.GetVehicleTypeRulesAsync(CancellationToken.None);

        Assert.Equal(rules, result);
        Assert.False(result[0].IsTollFree);
        Assert.True(result[1].IsTollFree);
    }
}
