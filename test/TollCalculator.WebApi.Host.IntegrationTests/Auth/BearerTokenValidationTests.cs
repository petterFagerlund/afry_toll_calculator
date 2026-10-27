using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using TollCalculator.WebApi.Host.Auth;
using TollCalculator.WebApi.Host.Configuration;
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
        var token = CreateToken(Issuer, Audience, SigningKey, TimeSpan.FromMinutes(60));

        var result = await new JsonWebTokenHandler().ValidateTokenAsync(token, _validationParameters);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ATokenSignedWithAnotherKey_IsRejected()
    {
        var token = CreateToken(Issuer, Audience, "a-completely-different-key-of-sufficient-length", TimeSpan.FromMinutes(60));

        var result = await new JsonWebTokenHandler().ValidateTokenAsync(token, _validationParameters);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ATokenFromAnotherIssuer_IsRejected()
    {
        var token = CreateToken("someone-else", Audience, SigningKey, TimeSpan.FromMinutes(60));

        var result = await new JsonWebTokenHandler().ValidateTokenAsync(token, _validationParameters);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ATokenForAnotherAudience_IsRejected()
    {
        var token = CreateToken(Issuer, "another-api", SigningKey, TimeSpan.FromMinutes(60));

        var result = await new JsonWebTokenHandler().ValidateTokenAsync(token, _validationParameters);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task AnExpiredToken_IsRejected()
    {
        var token = CreateToken(Issuer, Audience, SigningKey, TimeSpan.FromMinutes(-5));

        var result = await new JsonWebTokenHandler().ValidateTokenAsync(token, _validationParameters);

        Assert.False(result.IsValid);
    }

    private static string CreateToken(string issuer, string audience, string signingKey, TimeSpan lifetime)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = issuer,
            Audience = audience,
            Expires = DateTime.UtcNow.Add(lifetime),
            Subject = new ClaimsIdentity([new Claim(JwtRegisteredClaimNames.Sub, "toll-client")]),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }

    private static TokenValidationParameters BuildValidationParameters()
    {
        var jwtOptions = new JwtOptions
        {
            SigningKey = SigningKey,
            Issuer = Issuer,
            Audience = Audience,
            TokenLifetimeMinutes = 60,
        };

        var bearerOptions = new JwtBearerOptions();

        new JwtBearerOptionsSetup(Options.Create(jwtOptions))
            .Configure(JwtBearerDefaults.AuthenticationScheme, bearerOptions);

        return bearerOptions.TokenValidationParameters;
    }
}
