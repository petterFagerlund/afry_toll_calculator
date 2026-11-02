using TollCalculator.WebApi.Host.Validators.Implementations;
using TollCalculator.WebApi.Models.V1.Requests;

namespace TollCalculator.WebApi.Host.IntegrationTests.Validators;

public class CalculateDailyTollRequestValidatorTests
{
    private const string RegistrationNumber = "ABC123";

    private static readonly DateOnly Date = new(2026, 3, 16);

    [Fact]
    public void Validate_AcceptsARegistrationNumberAndDate()
    {
        var modelState = CalculateDailyTollRequestValidator.Validate(RequestOf(RegistrationNumber, Date));

        Assert.True(modelState.IsValid);
    }

    [Fact]
    public void Validate_RejectsAMissingRequest()
    {
        var modelState = CalculateDailyTollRequestValidator.Validate(null);

        Assert.False(modelState.IsValid);
        Assert.Contains("request", modelState.Keys);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_RejectsAMissingRegistrationNumber(string registrationNumber)
    {
        var modelState = CalculateDailyTollRequestValidator.Validate(RequestOf(registrationNumber, Date));

        Assert.False(modelState.IsValid);
        Assert.Contains("registrationNumber", modelState.Keys);
    }

    [Fact]
    public void Validate_RejectsARegistrationNumberLongerThanTheStoredColumn()
    {
        var modelState = CalculateDailyTollRequestValidator.Validate(RequestOf(new string('A', 21), Date));

        Assert.False(modelState.IsValid);
        Assert.Contains("registrationNumber", modelState.Keys);
    }

    [Fact]
    public void Validate_RejectsADateBeforeTheSupportedRange()
    {
        var modelState = CalculateDailyTollRequestValidator.Validate(
            RequestOf(RegistrationNumber, new DateOnly(1999, 12, 31)));

        Assert.False(modelState.IsValid);
        Assert.Contains("date", modelState.Keys);
    }

    [Fact]
    public void Validate_RejectsAnUnsetDate()
    {
        var modelState = CalculateDailyTollRequestValidator.Validate(
            new CalculateDailyTollRequest { RegistrationNumber = RegistrationNumber, Date = new DateOnly() });

        Assert.False(modelState.IsValid);
        Assert.Contains("date", modelState.Keys);
    }

    private static CalculateDailyTollRequest RequestOf(string registrationNumber, DateOnly date)
    {
        return new CalculateDailyTollRequest { RegistrationNumber = registrationNumber, Date = date };
    }
}
