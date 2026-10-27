using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using TollCalculator.WebApi.Host.OpenApi;
using TollCalculator.WebApi.Host.OpenApi.Transformers;
using Xunit;

namespace TollCalculator.WebApi.Host.IntegrationTests.OpenApi;

public class BearerSecuritySchemeDocumentTransformerTests
{
    private static async Task<OpenApiDocument> TransformedDocumentAsync()
    {
        var document = new OpenApiDocument();

        await new BearerSecuritySchemeDocumentTransformer().TransformAsync(document, null!, CancellationToken.None);

        return document;
    }

    [Fact]
    public async Task TransformAsync_DescribesAnHttpBearerSchemeCarryingAJwt()
    {
        var document = await TransformedDocumentAsync();

        var schemes = document.Components!.SecuritySchemes!;
        var scheme = Assert.Contains(BearerSecuritySchemeDocumentTransformer.SchemeName, schemes);

        Assert.Equal(SecuritySchemeType.Http, scheme.Type);
        Assert.Equal("bearer", scheme.Scheme);
        Assert.Equal("JWT", scheme.BearerFormat);
    }

    [Fact]
    public async Task TransformAsync_AppliesTheSchemeToTheWholeDocument()
    {
        var document = await TransformedDocumentAsync();

        var requirement = Assert.Single(document.Security!);

        Assert.Single(requirement);
    }
}

public class ApiVersionInfoDocumentTransformerTests
{
    [Fact]
    public async Task TransformAsync_NamesTheApiAndItsVersion()
    {
        var document = new OpenApiDocument();

        await new ApiVersionInfoDocumentTransformer(ApiDocuments.V1)
            .TransformAsync(document, null!, CancellationToken.None);

        Assert.Equal(ApiVersionInfoDocumentTransformer.Title, document.Info!.Title);
        Assert.Equal(ApiDocuments.V1, document.Info.Version);
        Assert.False(string.IsNullOrWhiteSpace(document.Info.Description));
    }
}
