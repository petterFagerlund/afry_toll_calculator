using TollCalculator.WebApi.Host.Validators.Implementation;
using TollCalculator.WebApi.Models.V1.Requests;
using Xunit;

namespace TollCalculator.WebApi.Host.IntegrationTests.Validators;

public class TokenRequestValidatorTests
{
    private readonly TokenRequestValidator _validator = new();

    [Fact]
    public void Validate_AcceptsACompleteRequest()
    {
        var result = _validator.Validate(new TokenRequest { ClientId = "toll-client", ClientSecret = "a-secret" });

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_RejectsAMissingRequest()
    {
        var result = _validator.Validate(null);

        Assert.False(result.IsValid);
        Assert.Contains(nameof(TokenRequest), result.Errors.Keys);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_RejectsAMissingClientId(string? clientId)
    {
        var result = _validator.Validate(new TokenRequest { ClientId = clientId, ClientSecret = "a-secret" });

        Assert.False(result.IsValid);
        Assert.Contains(nameof(TokenRequest.ClientId), result.Errors.Keys);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_RejectsAMissingClientSecret(string? clientSecret)
    {
        var result = _validator.Validate(new TokenRequest { ClientId = "toll-client", ClientSecret = clientSecret });

        Assert.False(result.IsValid);
        Assert.Contains(nameof(TokenRequest.ClientSecret), result.Errors.Keys);
    }

    [Fact]
    public void Validate_ReportsEveryBrokenRuleAtOnce()
    {
        var result = _validator.Validate(new TokenRequest());

        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count);
    }
}
