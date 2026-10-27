using TollCalculator.WebApi.Host.Validators.Implementations;
using TollCalculator.WebApi.Models.V1.Requests;
using Xunit;

namespace TollCalculator.WebApi.Host.IntegrationTests.Validators;

public class TokenRequestValidatorTests
{
    [Fact]
    public void Validate_AcceptsACompleteRequest()
    {
        var result = TokenRequestValidator.Validate(new TokenRequest { ClientId = "toll-client", ClientSecret = "a-secret" });

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_RejectsAMissingRequest()
    {
        var result = TokenRequestValidator.Validate(null);

        Assert.False(result.IsValid);
        Assert.Contains(nameof(TokenRequest), result.Errors.Keys);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_RejectsAMissingClientId(string? clientId)
    {
        var result = TokenRequestValidator.Validate(new TokenRequest { ClientId = clientId, ClientSecret = "a-secret" });

        Assert.False(result.IsValid);
        Assert.Contains(nameof(TokenRequest.ClientId), result.Errors.Keys);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_RejectsAMissingClientSecret(string? clientSecret)
    {
        var result = TokenRequestValidator.Validate(new TokenRequest { ClientId = "toll-client", ClientSecret = clientSecret });

        Assert.False(result.IsValid);
        Assert.Contains(nameof(TokenRequest.ClientSecret), result.Errors.Keys);
    }

    [Fact]
    public void Validate_ReportsEveryBrokenRuleAtOnce()
    {
        var result = TokenRequestValidator.Validate(new TokenRequest());

        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count);
    }
}
