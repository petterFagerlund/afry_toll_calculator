using TollCalculator.WebApi.Host.Validators.Implementations;
using TollCalculator.WebApi.Models.V1.Requests;
using Xunit;

namespace TollCalculator.WebApi.Host.IntegrationTests.Validators;

public class TokenRequestValidatorTests
{
    [Fact]
    public void Validate_AcceptsACompleteRequest()
    {
        var modelState = TokenRequestValidator.Validate(
            new TokenRequest { ClientId = "toll-client", ClientSecret = "a-secret" });

        Assert.True(modelState.IsValid);
        Assert.Empty(modelState);
    }

    [Fact]
    public void Validate_RejectsAMissingRequest()
    {
        var modelState = TokenRequestValidator.Validate(null);

        Assert.False(modelState.IsValid);
        Assert.Contains(nameof(TokenRequest), modelState.Keys);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_RejectsAMissingClientId(string? clientId)
    {
        var modelState = TokenRequestValidator.Validate(
            new TokenRequest { ClientId = clientId, ClientSecret = "a-secret" });

        Assert.False(modelState.IsValid);
        Assert.Contains(nameof(TokenRequest.ClientId), modelState.Keys);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_RejectsAMissingClientSecret(string? clientSecret)
    {
        var modelState = TokenRequestValidator.Validate(
            new TokenRequest { ClientId = "toll-client", ClientSecret = clientSecret });

        Assert.False(modelState.IsValid);
        Assert.Contains(nameof(TokenRequest.ClientSecret), modelState.Keys);
    }

    [Fact]
    public void Validate_ReportsEveryBrokenRuleAtOnce()
    {
        var modelState = TokenRequestValidator.Validate(new TokenRequest());

        Assert.False(modelState.IsValid);
        Assert.Equal(2, modelState.Count);
    }
}
