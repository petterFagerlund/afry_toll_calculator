using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TollCalculator.WebApi.Host.Configuration;
using TollCalculator.WebApi.Host.IntegrationTests.Infrastructure;
using TollCalculator.WebApi.Models.V1.Requests;

namespace TollCalculator.WebApi.Host.IntegrationTests.Auth;

[Collection(TollApiCollection.Name)]
public class AuthTests
{
    private const string ProtectedRoute = "/api/v1/toll-calculations";

    private static readonly CalculateDailyTollRequest ProtectedRequest = new()
    {
        RegistrationNumber = "AUTH01",
        Date = new DateOnly(2026, 3, 16),
    };

    private readonly TollApiFixture _fixture;

    public AuthTests(TollApiFixture fixture)
    {
        ArgumentNullException.ThrowIfNull(fixture);

        _fixture = fixture;
    }

    [Fact]
    public async Task OutsideDevelopment_AProtectedRouteWithoutATokenIsRejected()
    {
        using var client = _fixture.Production.CreateClient();

        using var response = await client.PostAsJsonAsync(ProtectedRoute, ProtectedRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("Bearer", response.Headers.WwwAuthenticate.Single().Scheme);
    }

    [Fact]
    public async Task OutsideDevelopment_AValidTokenOpensTheProtectedRoute()
    {
        using var client = _fixture.Production.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestTokenBuilder.CreateForTheApiUnderTest());

        using var response = await client.PostAsJsonAsync(ProtectedRoute, ProtectedRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("AUTH01", await response.Content.ReadAsStringAsync(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task InDevelopment_TheSameRouteAnswersAnonymously()
    {
        using var client = _fixture.Development.CreateClient();

        using var response = await client.PostAsJsonAsync(ProtectedRoute, ProtectedRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }


    [Fact]
    public async Task OutsideDevelopment_ATamperedTokenIsRejected()
    {
        using var client = _fixture.Production.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestTokenBuilder.CreateForTheApiUnderTest() + "x");

        using var response = await client.PostAsJsonAsync(ProtectedRoute, ProtectedRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task OutsideDevelopment_ATokenSignedWithAnotherKeyIsRejected()
    {
        var foreignToken = TestTokenBuilder.Create(
            IntegrationTestFactory.Issuer,
            IntegrationTestFactory.Audience,
            "a-completely-different-key-of-sufficient-length",
            TimeSpan.FromMinutes(60));

        using var client = _fixture.Production.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", foreignToken);

        using var response = await client.PostAsJsonAsync(ProtectedRoute, ProtectedRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task OutsideDevelopment_AnExpiredTokenIsRejected()
    {
        var expiredToken = TestTokenBuilder.Create(
            IntegrationTestFactory.Issuer,
            IntegrationTestFactory.Audience,
            IntegrationTestFactory.SigningKey,
            TimeSpan.FromMinutes(-5));

        using var client = _fixture.Production.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", expiredToken);

        using var response = await client.PostAsJsonAsync(ProtectedRoute, ProtectedRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("Development")]
    [InlineData("Production")]
    public async Task TheHealthEndpoint_StaysAnonymousInEveryEnvironment(string environmentName)
    {
        using var client = FactoryFor(environmentName).CreateClient();

        using var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task OutsideDevelopment_TheApiReferenceIsNotPublished()
    {
        using var client = _fixture.Production.CreateClient();

        using var scalar = await client.GetAsync("/scalar");
        using var document = await client.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.Unauthorized, scalar.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, document.StatusCode);
    }

    [Theory]
    [InlineData("Development")]
    [InlineData("Production")]
    public void TheApplicationUnderTest_UsesTheJwtSettingsTheFactoryConfigures(string environmentName)
    {
        using var scope = FactoryFor(environmentName).Services.CreateScope();

        var options = scope.ServiceProvider.GetRequiredService<IOptions<JwtOptions>>().Value;

        Assert.Equal(IntegrationTestFactory.SigningKey, options.SigningKey);
        Assert.Equal(IntegrationTestFactory.Issuer, options.Issuer);
        Assert.Equal(IntegrationTestFactory.Audience, options.Audience);
    }

    private IntegrationTestFactory FactoryFor(string environmentName)
    {
        if (environmentName == "Development")
        {
            return _fixture.Development;
        }

        return _fixture.Production;
    }
}
