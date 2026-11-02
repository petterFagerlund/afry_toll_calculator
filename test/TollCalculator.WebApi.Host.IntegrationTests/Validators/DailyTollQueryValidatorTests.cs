using TollCalculator.WebApi.Host.Validators.Implementations;
using Xunit;

namespace TollCalculator.WebApi.Host.IntegrationTests.Validators;

public class DailyTollQueryValidatorTests
{
    private const string RegistrationNumber = "ABC123";
    private const string DateMember = "date";

    [Fact]
    public void Validate_AcceptsARegistrationNumberAndDate()
    {
        var modelState = DailyTollQueryValidator.Validate(RegistrationNumber, new DateOnly(2026, 3, 16));

        Assert.True(modelState.IsValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_RejectsAMissingRegistrationNumber(string? registrationNumber)
    {
        var modelState = DailyTollQueryValidator.Validate(registrationNumber, new DateOnly(2026, 3, 16));

        Assert.False(modelState.IsValid);
        Assert.Contains("registrationNumber", modelState.Keys);
    }

    [Fact]
    public void Validate_RejectsARegistrationNumberLongerThanTheStoredColumn()
    {
        var modelState = DailyTollQueryValidator.Validate(new string('A', 21), new DateOnly(2026, 3, 16));

        Assert.False(modelState.IsValid);
        Assert.Contains("registrationNumber", modelState.Keys);
    }

    [Fact]
    public void Validate_RejectsAMissingDate()
    {
        var modelState = DailyTollQueryValidator.Validate(RegistrationNumber, null);

        Assert.False(modelState.IsValid);
        Assert.Contains(DateMember, modelState.Keys);
    }
}
