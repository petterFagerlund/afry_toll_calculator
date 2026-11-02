using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using TollCalculator.WebApi.Host.Auth;
using TollCalculator.WebApi.Host.Configuration;
using TollCalculator.WebApi.Host.IntegrationTests.Infrastructure;
using Xunit;

namespace TollCalculator.WebApi.Host.IntegrationTests.Auth;

public class BearerTokenValidationTests
{
    private const string SigningKey = "a-signing-key-that-is-long-enough-for-hmac-sha256";
    private const string Issuer = "toll-calculator";
    private const string Audience = "toll-calculator-api";

    private readonly TokenValidationParameters _validationParameters = BuildValidationParameters();

    [Fact]
    public async Task ATokenSignedWithTheConfiguredKey_IsAccepted()
    {
        var token = TestTokenBuilder.Create(Issuer, Audience, SigningKey, TimeSpan.FromMinutes(60));

        var result = await new JsonWebTokenHandler().ValidateTokenAsync(token, _validationParameters);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ATokenSignedWithAnotherKey_IsRejected()
    {
        var token = TestTokenBuilder.Create(
            Issuer, Audience, "a-completely-different-key-of-sufficient-length", TimeSpan.FromMinutes(60));

        var result = await new JsonWebTokenHandler().ValidateTokenAsync(token, _validationParameters);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ATokenFromAnotherIssuer_IsRejected()
    {
        var token = TestTokenBuilder.Create("someone-else", Audience, SigningKey, TimeSpan.FromMinutes(60));

        var result = await new JsonWebTokenHandler().ValidateTokenAsync(token, _validationParameters);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ATokenForAnotherAudience_IsRejected()
    {
        var token = TestTokenBuilder.Create(Issuer, "another-api", SigningKey, TimeSpan.FromMinutes(60));

        var result = await new JsonWebTokenHandler().ValidateTokenAsync(token, _validationParameters);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task AnExpiredToken_IsRejected()
    {
        var token = TestTokenBuilder.Create(Issuer, Audience, SigningKey, TimeSpan.FromMinutes(-5));

        var result = await new JsonWebTokenHandler().ValidateTokenAsync(token, _validationParameters);

        Assert.False(result.IsValid);
    }

    private static TokenValidationParameters BuildValidationParameters()
    {
        var jwtOptions = new JwtOptions
        {
            SigningKey = SigningKey,
            Issuer = Issuer,
            Audience = Audience,
        };

        var bearerOptions = new JwtBearerOptions();

        new JwtBearerOptionsSetup(Options.Create(jwtOptions))
            .Configure(JwtBearerDefaults.AuthenticationScheme, bearerOptions);

        return bearerOptions.TokenValidationParameters;
    }
}
