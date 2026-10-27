namespace TollCalculator.WebApi.Host.Validators.Implementation;

internal sealed class DailyTollQueryValidator : IDailyTollQueryValidator
{
    private const string RegistrationNumberMember = "registrationNumber";
    private const string DateMember = "date";

    public ValidationResult Validate(string? registrationNumber, DateOnly? date)
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
