using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TollCalculator.WebApi.Host.Auth;
using TollCalculator.WebApi.Host.Configuration;
using Xunit;

namespace TollCalculator.WebApi.Host.IntegrationTests.Auth;

public class JwtBearerOptionsSetupTests
{
    private const string SigningKey = "a-signing-key-that-is-long-enough-for-hmac-sha256";
    private const string Issuer = "toll-calculator";
    private const string Audience = "toll-calculator-api";

    private readonly JwtBearerOptions _bearerOptions = new();

    public JwtBearerOptionsSetupTests()
    {
        var jwtOptions = new JwtOptions
        {
            SigningKey = SigningKey,
            Issuer = Issuer,
            Audience = Audience,
            TokenLifetimeMinutes = 60,
        };

        new JwtBearerOptionsSetup(Options.Create(jwtOptions))
            .Configure(JwtBearerDefaults.AuthenticationScheme, _bearerOptions);
    }

    [Fact]
    public void Configure_ValidatesTheIssuerFromConfiguration()
    {
        Assert.True(_bearerOptions.TokenValidationParameters.ValidateIssuer);
        Assert.Equal(Issuer, _bearerOptions.TokenValidationParameters.ValidIssuer);
    }

    [Fact]
    public void Configure_ValidatesTheAudienceFromConfiguration()
    {
        Assert.True(_bearerOptions.TokenValidationParameters.ValidateAudience);
        Assert.Equal(Audience, _bearerOptions.TokenValidationParameters.ValidAudience);
    }

    [Fact]
    public void Configure_ValidatesTheSignatureWithTheConfiguredSymmetricKey()
    {
        Assert.True(_bearerOptions.TokenValidationParameters.ValidateIssuerSigningKey);

        var key = Assert.IsType<SymmetricSecurityKey>(_bearerOptions.TokenValidationParameters.IssuerSigningKey);

        Assert.Equal(Encoding.UTF8.GetBytes(SigningKey), key.Key);
    }

    [Fact]
    public void Configure_ValidatesTheLifetimeWithoutAGraceWindow()
    {
        Assert.True(_bearerOptions.TokenValidationParameters.ValidateLifetime);
        Assert.Equal(TimeSpan.Zero, _bearerOptions.TokenValidationParameters.ClockSkew);
    }

    [Fact]
    public void Configure_RequiresTheTokenToBeSentOverHttps()
    {
        Assert.True(_bearerOptions.RequireHttpsMetadata);
    }

    [Fact]
    public void Configure_IgnoresAnUnknownScheme()
    {
        var untouched = new JwtBearerOptions();
        var jwtOptions = new JwtOptions
        {
            SigningKey = SigningKey,
            Issuer = Issuer,
            Audience = Audience,
        };

        new JwtBearerOptionsSetup(Options.Create(jwtOptions)).Configure("some-other-scheme", untouched);

        Assert.Null(untouched.TokenValidationParameters.ValidIssuer);
    }
}
