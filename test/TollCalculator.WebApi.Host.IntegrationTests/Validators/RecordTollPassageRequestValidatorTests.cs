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
        var modelState = RecordTollPassageRequestValidator.Validate(new RecordTollPassageRequest
        {
            RegistrationNumber = "ABC123",
            PassedAt = PassedAt,
        });

        Assert.True(modelState.IsValid);
        Assert.Empty(modelState);
    }

    [Fact]
    public void Validate_RejectsAMaximumPassedAt()
    {
        var modelState = RecordTollPassageRequestValidator.Validate(new RecordTollPassageRequest
        {
            RegistrationNumber = "ABC123",
            PassedAt = DateTimeOffset.MaxValue,
        });

        Assert.False(modelState.IsValid);
        Assert.Contains(nameof(RecordTollPassageRequest.PassedAt), modelState.Keys);
    }

    [Fact]
    public void Validate_RejectsAMissingRequest()
    {
        var modelState = RecordTollPassageRequestValidator.Validate(null);

        Assert.False(modelState.IsValid);
        Assert.Contains(nameof(RecordTollPassageRequest), modelState.Keys);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_RejectsAMissingRegistrationNumber(string? registrationNumber)
    {
        var modelState = RecordTollPassageRequestValidator.Validate(new RecordTollPassageRequest
        {
            RegistrationNumber = registrationNumber,
            PassedAt = PassedAt,
        });

        Assert.False(modelState.IsValid);
        Assert.Contains(nameof(RecordTollPassageRequest.RegistrationNumber), modelState.Keys);
    }

    [Fact]
    public void Validate_RejectsARegistrationNumberLongerThanTheStoredColumn()
    {
        var modelState = RecordTollPassageRequestValidator.Validate(new RecordTollPassageRequest
        {
            RegistrationNumber = new string('A', 21),
            PassedAt = PassedAt,
        });

        Assert.False(modelState.IsValid);
        Assert.Contains(nameof(RecordTollPassageRequest.RegistrationNumber), modelState.Keys);
    }

    [Fact]
    public void Validate_AcceptsARegistrationNumberAtTheMaximumLength()
    {
        var modelState = RecordTollPassageRequestValidator.Validate(new RecordTollPassageRequest
        {
            RegistrationNumber = new string('A', 20),
            PassedAt = PassedAt,
        });

        Assert.True(modelState.IsValid);
    }

    [Fact]
    public void Validate_RejectsAnUnsetTimestamp()
    {
        var modelState = RecordTollPassageRequestValidator.Validate(
            new RecordTollPassageRequest { RegistrationNumber = "ABC123" });

        Assert.False(modelState.IsValid);
        Assert.Contains(nameof(RecordTollPassageRequest.PassedAt), modelState.Keys);
    }

    [Fact]
    public void Validate_ReportsEveryBrokenRuleAtOnce()
    {
        var modelState = RecordTollPassageRequestValidator.Validate(new RecordTollPassageRequest());

        Assert.False(modelState.IsValid);
        Assert.Equal(2, modelState.Count);
    }
}
