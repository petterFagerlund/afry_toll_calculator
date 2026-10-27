using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TollCalculator.WebApi.Host.Configuration;
using TollCalculator.WebApi.Host.Registrations;
using Xunit;

namespace TollCalculator.WebApi.Host.IntegrationTests.Configuration;

public class JwtOptionsTests
{
    private const string SigningKey = "a-development-signing-key-of-at-least-32-characters";
    private const string ClientId = "toll-client";
    private const string ClientSecret = "a-client-secret-long-enough";

    [Fact]
    public void AddJwtOptions_BindsTheJwtSection()
    {
        var options = Resolve(BuildSection(SigningKey, "toll-calculator", "toll-calculator-api", "60"));

        Assert.Equal("toll-calculator", options.Issuer);
        Assert.Equal("toll-calculator-api", options.Audience);
        Assert.Equal(SigningKey, options.SigningKey);
        Assert.Equal(60, options.TokenLifetimeMinutes);
        Assert.Equal(ClientId, options.ClientId);
        Assert.Equal(ClientSecret, options.ClientSecret);
    }

    [Fact]
    public void JwtOptions_ExposesTheSectionNameItBindsTo()
    {
        Assert.Equal("Jwt", JwtOptions.SectionName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("too-short")]
    public void AddJwtOptions_RejectsASigningKeyShorterThanTheRequiredLength(string? signingKey)
    {
        var configuration = BuildSection(signingKey, "toll-calculator", "toll-calculator-api", "60");

        Assert.Throws<OptionsValidationException>(() => Resolve(configuration));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void AddJwtOptions_RejectsAMissingIssuer(string? issuer)
    {
        var configuration = BuildSection(SigningKey, issuer, "toll-calculator-api", "60");

        Assert.Throws<OptionsValidationException>(() => Resolve(configuration));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void AddJwtOptions_RejectsAMissingAudience(string? audience)
    {
        var configuration = BuildSection(SigningKey, "toll-calculator", audience, "60");

        Assert.Throws<OptionsValidationException>(() => Resolve(configuration));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-5")]
    [InlineData("1441")]
    public void AddJwtOptions_RejectsALifetimeOutsideTheSupportedRange(string lifetime)
    {
        var configuration = BuildSection(SigningKey, "toll-calculator", "toll-calculator-api", lifetime);

        Assert.Throws<OptionsValidationException>(() => Resolve(configuration));
    }

    [Fact]
    public void AddJwtOptions_RejectsAMissingJwtSection()
    {
        var configuration = new ConfigurationBuilder().Build();

        Assert.Throws<OptionsValidationException>(() => Resolve(configuration));
    }

    private static JwtOptions Resolve(IConfiguration configuration)
    {
        var services = new ServiceCollection();

        services.AddJwtOptions(configuration);

        using var provider = services.BuildServiceProvider();

        return provider.GetRequiredService<IOptions<JwtOptions>>().Value;
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void AddJwtOptions_RejectsAMissingClientId(string? clientId)
    {
        var configuration = BuildSection(SigningKey, "toll-calculator", "toll-calculator-api", "60", clientId, ClientSecret);

        Assert.Throws<OptionsValidationException>(() => Resolve(configuration));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("short")]
    public void AddJwtOptions_RejectsAClientSecretShorterThanTheRequiredLength(string? clientSecret)
    {
        var configuration = BuildSection(SigningKey, "toll-calculator", "toll-calculator-api", "60", ClientId, clientSecret);

        Assert.Throws<OptionsValidationException>(() => Resolve(configuration));
    }

    private static IConfiguration BuildSection(
        string? signingKey,
        string? issuer,
        string? audience,
        string lifetime,
        string? clientId = ClientId,
        string? clientSecret = ClientSecret)
    {
        var values = new Dictionary<string, string?>
        {
            [$"{JwtOptions.SectionName}:SigningKey"] = signingKey,
            [$"{JwtOptions.SectionName}:Issuer"] = issuer,
            [$"{JwtOptions.SectionName}:Audience"] = audience,
            [$"{JwtOptions.SectionName}:TokenLifetimeMinutes"] = lifetime,
            [$"{JwtOptions.SectionName}:ClientId"] = clientId,
            [$"{JwtOptions.SectionName}:ClientSecret"] = clientSecret,
        };

        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }
}
