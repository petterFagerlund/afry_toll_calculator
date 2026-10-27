using System.Reflection;
using Asp.Versioning;
using TollCalculator.WebApi.Host.Controllers;
using TollCalculator.WebApi.Host.OpenApi;
using Xunit;

namespace TollCalculator.WebApi.Host.IntegrationTests.OpenApi;

public class ApiDocumentsTests
{
    [Fact]
    public void EveryVersionDeclaredOnAControllerHasAnOpenApiDocument()
    {
        var declaredDocuments = typeof(ApiControllerBase).Assembly.GetTypes()
            .Where(type => type.IsAssignableTo(typeof(ApiControllerBase)) && !type.IsAbstract)
            .SelectMany(type => type.GetCustomAttributes<ApiVersionAttribute>())
            .SelectMany(attribute => attribute.Versions)
            .Select(version => $"v{version.MajorVersion}")
            .Distinct();

        Assert.All(declaredDocuments, document => Assert.Contains(document, ApiDocuments.All));
    }

    [Fact]
    public void EveryControllerDeclaresAVersion()
    {
        var unversioned = typeof(ApiControllerBase).Assembly.GetTypes()
            .Where(type => type.IsAssignableTo(typeof(ApiControllerBase)) && !type.IsAbstract)
            .Where(type => type.GetCustomAttributes<ApiVersionAttribute>().Any() == false)
            .Select(type => type.Name);

        Assert.Empty(unversioned);
    }
}
