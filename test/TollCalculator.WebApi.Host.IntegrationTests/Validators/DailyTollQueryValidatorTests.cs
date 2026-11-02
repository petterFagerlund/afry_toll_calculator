using TollCalculator.WebApi.Host.Validators.Implementations;

namespace TollCalculator.WebApi.Host.IntegrationTests.Validators;

public class DailyTollQueryValidatorTests
{
    private const string RegistrationNumber = "ABC123";

    [Fact]
    public void Validate_AcceptsARegistrationNumber()
    {
        var modelState = DailyTollQueryValidator.Validate(RegistrationNumber);

        Assert.True(modelState.IsValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_RejectsAMissingRegistrationNumber(string? registrationNumber)
    {
        var modelState = DailyTollQueryValidator.Validate(registrationNumber);

        Assert.False(modelState.IsValid);
        Assert.Contains("registrationNumber", modelState.Keys);
    }

    [Fact]
    public void Validate_RejectsARegistrationNumberLongerThanTheStoredColumn()
    {
        var modelState = DailyTollQueryValidator.Validate(new string('A', 21));

        Assert.False(modelState.IsValid);
        Assert.Contains("registrationNumber", modelState.Keys);
    }
}
