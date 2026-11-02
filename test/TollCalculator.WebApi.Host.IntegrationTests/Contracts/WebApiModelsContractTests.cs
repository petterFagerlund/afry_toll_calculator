using System.Reflection;
using TollCalculator.WebApi.Models.V1.Requests;
using Xunit;
using ContractVehicleType = TollCalculator.WebApi.Models.V1.VehicleType;

namespace TollCalculator.WebApi.Host.IntegrationTests.Contracts;

public class WebApiModelsContractTests
{
    private static readonly Assembly ContractAssembly = typeof(RecordTollPassageRequest).Assembly;

    [Fact]
    public void TheContractAssembly_ReferencesNoProjectAssembly()
    {
        var referenced = ContractAssembly.GetReferencedAssemblies()
            .Where(assembly => assembly.Name?.StartsWith("TollCalculator", StringComparison.Ordinal) == true);

        Assert.Empty(referenced);
    }

    [Fact]
    public void TheContractAssembly_DeclaresVehicleTypeAsItsOnlyEnum()
    {
        var enums = ContractAssembly.GetTypes().Where(type => type.IsEnum);

        Assert.Equal(typeof(ContractVehicleType), Assert.Single(enums));
    }

    [Fact]
    public void ContractVehicleType_DeclaresNoUnknownMember()
    {
        Assert.DoesNotContain("Unknown", Enum.GetNames<ContractVehicleType>());
    }

    [Fact]
    public void ContractVehicleType_LeavesItsDefaultValueUndefined()
    {
        Assert.DoesNotContain(0, Enum.GetValues<ContractVehicleType>().Select(value => (int)value));
        Assert.False(Enum.IsDefined(new ContractVehicleType()));
    }
}
