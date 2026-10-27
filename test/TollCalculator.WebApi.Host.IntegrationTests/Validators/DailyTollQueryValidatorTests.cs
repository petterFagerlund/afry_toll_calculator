using TollCalculator.WebApi.Host.Validators.Implementation;
using Xunit;

namespace TollCalculator.WebApi.Host.IntegrationTests.Validators;

public class DailyTollQueryValidatorTests
{
    private const string RegistrationNumber = "ABC123";
    private const string DateMember = "date";

    private readonly DailyTollQueryValidator _validator = new();

    [Fact]
    public void Validate_AcceptsARegistrationNumberAndDate()
    {
        var result = _validator.Validate(RegistrationNumber, new DateOnly(2026, 3, 16));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_RejectsAMissingRegistrationNumber(string? registrationNumber)
    {
        var result = _validator.Validate(registrationNumber, new DateOnly(2026, 3, 16));

        Assert.False(result.IsValid);
        Assert.Contains("registrationNumber", result.Errors.Keys);
    }

    [Fact]
    public void Validate_RejectsARegistrationNumberLongerThanTheStoredColumn()
    {
        var result = _validator.Validate(new string('A', 21), new DateOnly(2026, 3, 16));

        Assert.False(result.IsValid);
        Assert.Contains("registrationNumber", result.Errors.Keys);
    }

    [Fact]
    public void Validate_RejectsAMissingDate()
    {
        var result = _validator.Validate(RegistrationNumber, null);

        Assert.False(result.IsValid);
        Assert.Contains(DateMember, result.Errors.Keys);
    }
}
