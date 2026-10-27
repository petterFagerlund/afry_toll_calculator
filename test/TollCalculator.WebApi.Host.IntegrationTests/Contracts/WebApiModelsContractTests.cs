using System.Reflection;
using TollCalculator.WebApi.Models.V1.Requests;
using Xunit;

namespace TollCalculator.WebApi.Host.IntegrationTests.Contracts;

public class WebApiModelsContractTests
{
    private static readonly Assembly ContractAssembly = typeof(RecordTollPassageRequest).Assembly;

    [Fact]
    public void TheContractAssembly_DeclaresNoEnums()
    {
        var enums = ContractAssembly.GetTypes().Where(type => type.IsEnum).Select(type => type.FullName);

        Assert.Empty(enums);
    }

    [Fact]
    public void NoContractProperty_ExposesAnEnum()
    {
        var offenders = new List<string>();

        foreach (var type in ContractAssembly.GetTypes().Where(type => type.IsClass))
        {
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                if (propertyType.IsEnum)
                {
                    offenders.Add($"{type.Name}.{property.Name}");
                }
            }
        }

        Assert.Empty(offenders);
    }

    [Fact]
    public void TheContractAssembly_ReferencesNoProjectAssembly()
    {
        var referenced = ContractAssembly.GetReferencedAssemblies()
            .Where(assembly => assembly.Name?.StartsWith("TollCalculator", StringComparison.Ordinal) == true);

        Assert.Empty(referenced);
    }
}
