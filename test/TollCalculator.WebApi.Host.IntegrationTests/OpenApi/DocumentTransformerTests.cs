using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using TollCalculator.WebApi.Host.OpenApi;
using TollCalculator.WebApi.Host.OpenApi.Transformers;
using Xunit;

namespace TollCalculator.WebApi.Host.IntegrationTests.OpenApi;

public class BearerSecuritySchemeDocumentTransformerTests
{
    private readonly OpenApiDocument _document = new();

    public BearerSecuritySchemeDocumentTransformerTests()
    {
        new BearerSecuritySchemeDocumentTransformer()
            .TransformAsync(_document, null!, CancellationToken.None)
            .GetAwaiter()
            .GetResult();
    }

    [Fact]
    public void TransformAsync_DescribesAnHttpBearerSchemeCarryingAJwt()
    {
        var scheme = Assert.Contains(BearerSecuritySchemeDocumentTransformer.SchemeName, _document.Components!.SecuritySchemes);

        Assert.Equal(SecuritySchemeType.Http, scheme.Type);
        Assert.Equal("bearer", scheme.Scheme);
        Assert.Equal("JWT", scheme.BearerFormat);
    }

    [Fact]
    public void TransformAsync_AppliesTheSchemeToTheWholeDocument()
    {
        var requirement = Assert.Single(_document.Security!);

        Assert.Single(requirement);
    }
}

public class ApiVersionInfoDocumentTransformerTests
{
    [Fact]
    public void TransformAsync_NamesTheApiAndItsVersion()
    {
        var document = new OpenApiDocument();

        new ApiVersionInfoDocumentTransformer(ApiDocuments.V1)
            .TransformAsync(document, null!, CancellationToken.None)
            .GetAwaiter()
            .GetResult();

        Assert.Equal(ApiVersionInfoDocumentTransformer.Title, document.Info!.Title);
        Assert.Equal(ApiDocuments.V1, document.Info.Version);
        Assert.False(string.IsNullOrWhiteSpace(document.Info.Description));
    }
}
