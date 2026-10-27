using TollCalculator.WebApi.Host.Validators.Implementations;
using TollCalculator.WebApi.Models.V1.Requests;
using Xunit;

namespace TollCalculator.WebApi.Host.IntegrationTests.Validators;

public class RecordTollPassageRequestValidatorTests
{
    private static readonly DateTimeOffset PassedAt = new(2026, 3, 16, 7, 15, 0, TimeSpan.FromHours(1));

    [Fact]
    public void Validate_AcceptsACompleteRequest()
    {
        var result = RecordTollPassageRequestValidator.Validate(new RecordTollPassageRequest
        {
            RegistrationNumber = "ABC123",
            PassedAt = PassedAt,
        });

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_RejectsAMissingRequest()
    {
        var result = RecordTollPassageRequestValidator.Validate(null);

        Assert.False(result.IsValid);
        Assert.Contains(nameof(RecordTollPassageRequest), result.Errors.Keys);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_RejectsAMissingRegistrationNumber(string? registrationNumber)
    {
        var result = RecordTollPassageRequestValidator.Validate(new RecordTollPassageRequest
        {
            RegistrationNumber = registrationNumber,
            PassedAt = PassedAt,
        });

        Assert.False(result.IsValid);
        Assert.Contains(nameof(RecordTollPassageRequest.RegistrationNumber), result.Errors.Keys);
    }

    [Fact]
    public void Validate_RejectsARegistrationNumberLongerThanTheStoredColumn()
    {
        var result = RecordTollPassageRequestValidator.Validate(new RecordTollPassageRequest
        {
            RegistrationNumber = new string('A', 21),
            PassedAt = PassedAt,
        });

        Assert.False(result.IsValid);
        Assert.Contains(nameof(RecordTollPassageRequest.RegistrationNumber), result.Errors.Keys);
    }

    [Fact]
    public void Validate_AcceptsARegistrationNumberAtTheMaximumLength()
    {
        var result = RecordTollPassageRequestValidator.Validate(new RecordTollPassageRequest
        {
            RegistrationNumber = new string('A', 20),
            PassedAt = PassedAt,
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_RejectsAnUnsetTimestamp()
    {
        var result = RecordTollPassageRequestValidator.Validate(new RecordTollPassageRequest { RegistrationNumber = "ABC123" });

        Assert.False(result.IsValid);
        Assert.Contains(nameof(RecordTollPassageRequest.PassedAt), result.Errors.Keys);
    }

    [Fact]
    public void Validate_ReportsEveryBrokenRuleAtOnce()
    {
        var result = RecordTollPassageRequestValidator.Validate(new RecordTollPassageRequest());

        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count);
    }
}
