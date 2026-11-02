using Microsoft.AspNetCore.Mvc.ModelBinding;
using TollCalculator.WebApi.Models.V1.Requests;

namespace TollCalculator.WebApi.Host.Validators.Implementations;

internal static class CalculateDailyTollRequestValidator
{
    private const string RequestMember = "request";
    private const string RegistrationNumberMember = "registrationNumber";
    private const string DateMember = "date";

    private static readonly DateOnly EarliestAccepted = new(2000, 1, 1);

    public static ModelStateDictionary Validate(CalculateDailyTollRequest? request)
    {
        var modelState = new ModelStateDictionary();

        if (request is null)
        {
            modelState.AddModelError(RequestMember, "A request body is required.");

            return modelState;
        }

        var registrationNumberError = RegistrationNumberRule.Check(request.RegistrationNumber);

        if (registrationNumberError is not null)
        {
            modelState.AddModelError(RegistrationNumberMember, registrationNumberError);
        }

        if (request.Date < EarliestAccepted)
        {
            modelState.AddModelError(DateMember, $"A date cannot be earlier than {EarliestAccepted:yyyy-MM-dd}.");
        }

        return modelState;
    }
}
