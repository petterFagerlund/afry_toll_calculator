namespace TollCalculator.WebApi.Host.Validators.Implementations;

internal static class DailyTollQueryValidator
{
    private const string RegistrationNumberMember = "registrationNumber";
    private const string DateMember = "date";

    public static ValidationResult Validate(string? registrationNumber, DateOnly? date)
    {
        var errors = new ValidationErrors();

        var registrationNumberError = RegistrationNumberRule.Check(registrationNumber);

        if (registrationNumberError is not null)
        {
            errors.Add(RegistrationNumberMember, registrationNumberError);
        }

        if (date is null)
        {
            errors.Add(DateMember, "A date in the format yyyy-MM-dd is required.");
        }

        return errors.ToResult();
    }
}
