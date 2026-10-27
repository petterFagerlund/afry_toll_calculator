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
        var result = DailyTollQueryValidator.Validate(RegistrationNumber, new DateOnly(2026, 3, 16));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_RejectsAMissingRegistrationNumber(string? registrationNumber)
    {
        var result = DailyTollQueryValidator.Validate(registrationNumber, new DateOnly(2026, 3, 16));

        Assert.False(result.IsValid);
        Assert.Contains("registrationNumber", result.Errors.Keys);
    }

    [Fact]
    public void Validate_RejectsARegistrationNumberLongerThanTheStoredColumn()
    {
        var result = DailyTollQueryValidator.Validate(new string('A', 21), new DateOnly(2026, 3, 16));

        Assert.False(result.IsValid);
        Assert.Contains("registrationNumber", result.Errors.Keys);
    }

    [Fact]
    public void Validate_RejectsAMissingDate()
    {
        var result = DailyTollQueryValidator.Validate(RegistrationNumber, null);

        Assert.False(result.IsValid);
        Assert.Contains(DateMember, result.Errors.Keys);
    }
}
