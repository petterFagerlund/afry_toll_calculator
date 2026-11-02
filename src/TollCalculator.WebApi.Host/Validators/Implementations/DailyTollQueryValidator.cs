using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TollCalculator.WebApi.Host.Validators.Implementations;

internal static class DailyTollQueryValidator
{
    private const string RegistrationNumberMember = "registrationNumber";

    public static ModelStateDictionary Validate(string? registrationNumber)
    {
        var modelState = new ModelStateDictionary();

        var registrationNumberError = RegistrationNumberRule.Check(registrationNumber);

        if (registrationNumberError is not null)
        {
            modelState.AddModelError(RegistrationNumberMember, registrationNumberError);
        }

        return modelState;
    }
}
