using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TollCalculator.WebApi.Host.Validators.Implementations;

internal static class DailyTollQueryValidator
{
    private const string RegistrationNumberMember = "registrationNumber";
    private const string DateMember = "date";

    public static ModelStateDictionary Validate(string? registrationNumber, DateOnly? date)
    {
        var modelState = new ModelStateDictionary();

        var registrationNumberError = RegistrationNumberRule.Check(registrationNumber);

        if (registrationNumberError is not null)
        {
            modelState.AddModelError(RegistrationNumberMember, registrationNumberError);
        }

        if (date is null)
        {
            modelState.AddModelError(DateMember, "A date in the format yyyy-MM-dd is required.");
        }

        return modelState;
    }
}
