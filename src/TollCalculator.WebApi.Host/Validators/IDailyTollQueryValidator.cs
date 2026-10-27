namespace TollCalculator.WebApi.Host.Validators;

public interface IDailyTollQueryValidator
{
    ValidationResult Validate(string? registrationNumber, DateOnly? date);
}
