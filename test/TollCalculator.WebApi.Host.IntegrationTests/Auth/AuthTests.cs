using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TollCalculator.WebApi.Host.Configuration;
using TollCalculator.WebApi.Host.IntegrationTests.Infrastructure;
using TollCalculator.WebApi.Models.V1.Requests;
using TollCalculator.WebApi.Models.V1.Responses;
using Xunit;

namespace TollCalculator.WebApi.Host.IntegrationTests.Auth;

[Collection(TollApiCollection.Name)]
public class AuthTests
{
    private const string ProtectedRoute = "/api/v1/toll-rates";
    private const string TokenRoute = "/api/v1/auth/token";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

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

        using var response = await client.GetAsync(ProtectedRoute);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("Bearer", response.Headers.WwwAuthenticate.Single().Scheme);
    }

    [Fact]
    public async Task OutsideDevelopment_ATokenFromTheTokenEndpointOpensTheProtectedRoute()
    {
        using var client = _fixture.Production.CreateClient();

        var token = await RequestTokenAsync(client);

        Assert.Equal("Bearer", token.TokenType);
        Assert.True(token.ExpiresAt > DateTimeOffset.UtcNow);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token.TokenType, token.AccessToken);

        using var response = await client.GetAsync(ProtectedRoute);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("06:00", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task InDevelopment_TheSameRouteAnswersAnonymously()
    {
        using var client = _fixture.Development.CreateClient();

        using var response = await client.GetAsync(ProtectedRoute);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task OutsideDevelopment_ATamperedTokenIsRejected()
    {
        using var client = _fixture.Production.CreateClient();

        var token = await RequestTokenAsync(client);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(token.TokenType, token.AccessToken + "x");

        using var response = await client.GetAsync(ProtectedRoute);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task TheTokenEndpoint_RejectsCredentialsThatDoNotMatch()
    {
        using var client = _fixture.Production.CreateClient();

        using var response = await PostTokenAsync(client, IntegrationTestFactory.ClientId, "not-the-right-secret");

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
    public void TheApplicationUnderTest_UsesTheCredentialsTheFactoryConfigures(string environmentName)
    {
        using var scope = FactoryFor(environmentName).Services.CreateScope();

        var options = scope.ServiceProvider.GetRequiredService<IOptions<JwtOptions>>().Value;

        Assert.Equal(IntegrationTestFactory.ClientId, options.ClientId);
        Assert.Equal(IntegrationTestFactory.ClientSecret, options.ClientSecret);
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

    private static async Task<TokenResponse> RequestTokenAsync(HttpClient client)
    {
        using var response = await PostTokenAsync(client, IntegrationTestFactory.ClientId,
            IntegrationTestFactory.ClientSecret);

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<TokenResponse>(SerializerOptions))!;
    }

    private static async Task<HttpResponseMessage> PostTokenAsync(
        HttpClient client,
        string clientId,
        string clientSecret)
    {
        var request = new TokenRequest { ClientId = clientId, ClientSecret = clientSecret };

        return await client.PostAsJsonAsync(TokenRoute, request, SerializerOptions);
    }
}
