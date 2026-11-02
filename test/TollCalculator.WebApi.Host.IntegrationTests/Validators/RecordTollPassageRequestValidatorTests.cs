using TollCalculator.WebApi.Host.Validators.Implementations;
using TollCalculator.WebApi.Models.V1.Requests;
using Xunit;

namespace TollCalculator.WebApi.Host.IntegrationTests.Validators;

public class RecordTollPassageRequestValidatorTests
{
    private static readonly DateTimeOffset PassedAt = new(2026, 3, 16, 7, 15, 0, TimeSpan.FromHours(1));
    private static readonly DateTimeOffset Now = new(2026, 3, 16, 12, 0, 0, TimeSpan.FromHours(1));

    [Fact]
    public void Validate_AcceptsACompleteRequest()
    {
        var modelState = RecordTollPassageRequestValidator.Validate(new RecordTollPassageRequest
        {
            RegistrationNumber = "ABC123",
            PassedAt = PassedAt,
        }, Now);

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
        }, Now);

        Assert.False(modelState.IsValid);
        Assert.Contains(nameof(RecordTollPassageRequest.PassedAt), modelState.Keys);
    }

    [Fact]
    public void Validate_RejectsAMissingRequest()
    {
        var modelState = RecordTollPassageRequestValidator.Validate(null, Now);

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
        }, Now);

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
        }, Now);

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
        }, Now);

        Assert.True(modelState.IsValid);
    }

    [Fact]
    public void Validate_RejectsAnUnsetTimestamp()
    {
        var modelState = RecordTollPassageRequestValidator.Validate(
            new RecordTollPassageRequest { RegistrationNumber = "ABC123" }, Now);

        Assert.False(modelState.IsValid);
        Assert.Contains(nameof(RecordTollPassageRequest.PassedAt), modelState.Keys);
    }

    [Fact]
    public void Validate_ReportsEveryBrokenRuleAtOnce()
    {
        var modelState = RecordTollPassageRequestValidator.Validate(new RecordTollPassageRequest(), Now);

        Assert.False(modelState.IsValid);
        Assert.Equal(2, modelState.Count);
    }

    [Fact]
    public void Validate_RejectsAPassedAtInTheFuture()
    {
        var modelState = RecordTollPassageRequestValidator.Validate(new RecordTollPassageRequest
        {
            RegistrationNumber = "ABC123",
            PassedAt = Now.AddHours(1),
        }, Now);

        Assert.False(modelState.IsValid);
        Assert.Contains(nameof(RecordTollPassageRequest.PassedAt), modelState.Keys);
    }

    [Fact]
    public void Validate_AcceptsAPassedAtWithinTheClockSkew()
    {
        var modelState = RecordTollPassageRequestValidator.Validate(new RecordTollPassageRequest
        {
            RegistrationNumber = "ABC123",
            PassedAt = Now.AddMinutes(1),
        }, Now);

        Assert.True(modelState.IsValid);
    }

    [Fact]
    public void Validate_RejectsAPassedAtBeforeTheSupportedRange()
    {
        var modelState = RecordTollPassageRequestValidator.Validate(new RecordTollPassageRequest
        {
            RegistrationNumber = "ABC123",
            PassedAt = new DateTimeOffset(1999, 12, 31, 23, 59, 59, TimeSpan.Zero),
        }, Now);

        Assert.False(modelState.IsValid);
        Assert.Contains(nameof(RecordTollPassageRequest.PassedAt), modelState.Keys);
    }
}
