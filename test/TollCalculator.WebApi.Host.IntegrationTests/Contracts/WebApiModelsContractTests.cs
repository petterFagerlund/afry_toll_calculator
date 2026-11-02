using System.Reflection;
using TollCalculator.WebApi.Models.V1.Requests;
using Xunit;

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
    public void TheContractAssembly_DeclaresNoEnums()
    {
        var enums = ContractAssembly.GetTypes().Where(type => type.IsEnum).Select(type => type.FullName);

        Assert.Empty(enums);
    }
}
