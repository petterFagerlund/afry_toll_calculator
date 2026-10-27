using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.IdentityModel.JsonWebTokens;
using TollCalculator.WebApi.Host.Auth;
using TollCalculator.WebApi.Host.Configuration;
using Xunit;

namespace TollCalculator.WebApi.Host.IntegrationTests.Auth;

public class JwtTokenIssuerTests
{
    private const string SigningKey = "a-signing-key-that-is-long-enough-for-hmac-sha256";
    private const string Issuer = "toll-calculator";
    private const string Audience = "toll-calculator-api";
    private const string ClientId = "toll-client";
    private const string ClientSecret = "a-client-secret-long-enough";

    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;

    private readonly FakeTimeProvider _timeProvider = new(Now);
    private readonly JwtTokenIssuer _issuer;

    public JwtTokenIssuerTests()
    {
        _issuer = new JwtTokenIssuer(Options.Create(BuildOptions()), _timeProvider);
    }

    [Fact]
    public void Issue_ReturnsABearerTokenForTheConfiguredCredentials()
    {
        var issued = _issuer.Issue(ClientId, ClientSecret);

        Assert.NotNull(issued);
        Assert.Equal("Bearer", issued.TokenType);
        Assert.False(string.IsNullOrWhiteSpace(issued.AccessToken));
    }

    [Fact]
    public void Issue_SetsTheExpiryFromTheConfiguredLifetime()
    {
        var issued = _issuer.Issue(ClientId, ClientSecret);

        Assert.Equal(Now.AddMinutes(30), issued!.ExpiresAt);
    }

    [Theory]
    [InlineData("another-client", ClientSecret)]
    [InlineData(ClientId, "another-secret-long-enough")]
    [InlineData("", "")]
    public void Issue_ReturnsNothingForCredentialsThatDoNotMatch(string clientId, string clientSecret)
    {
        Assert.Null(_issuer.Issue(clientId, clientSecret));
    }

    [Fact]
    public async Task Issue_ProducesATokenTheBearerHandlerAccepts()
    {
        var issued = _issuer.Issue(ClientId, ClientSecret);

        var result = await new JsonWebTokenHandler().ValidateTokenAsync(issued!.AccessToken, BuildValidationParameters());

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Issue_NamesTheClientAsTheSubject()
    {
        var issued = _issuer.Issue(ClientId, ClientSecret);

        var result = await new JsonWebTokenHandler().ValidateTokenAsync(issued!.AccessToken, BuildValidationParameters());

        Assert.Equal(ClientId, result.ClaimsIdentity.FindFirst(JwtRegisteredClaimNames.Sub)?.Value);
    }

    private static JwtOptions BuildOptions()
    {
        return new JwtOptions
        {
            SigningKey = SigningKey,
            Issuer = Issuer,
            Audience = Audience,
            TokenLifetimeMinutes = 30,
            ClientId = ClientId,
            ClientSecret = ClientSecret,
        };
    }

    private static Microsoft.IdentityModel.Tokens.TokenValidationParameters BuildValidationParameters()
    {
        var bearerOptions = new JwtBearerOptions();

        new JwtBearerOptionsSetup(Options.Create(BuildOptions()))
            .Configure(JwtBearerDefaults.AuthenticationScheme, bearerOptions);

        return bearerOptions.TokenValidationParameters;
    }
}
