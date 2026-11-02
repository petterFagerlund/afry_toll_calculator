using Microsoft.AspNetCore.Mvc.ModelBinding;
using TollCalculator.WebApi.Models.V1.Requests;

namespace TollCalculator.WebApi.Host.Validators.Implementations;

internal static class RecordTollPassageRequestValidator
{
    public static ModelStateDictionary Validate(RecordTollPassageRequest? request)
    {
        var modelState = new ModelStateDictionary();

        if (request is null)
        {
            modelState.AddModelError(nameof(RecordTollPassageRequest), "A request body is required.");

            return modelState;
        }

        var registrationNumberError = RegistrationNumberRule.Check(request.RegistrationNumber);

        if (registrationNumberError is not null)
        {
            modelState.AddModelError(nameof(RecordTollPassageRequest.RegistrationNumber), registrationNumberError);
        }

        if (request.PassedAt == DateTimeOffset.MinValue)
        {
            modelState.AddModelError(nameof(RecordTollPassageRequest.PassedAt), "A passage timestamp is required.");
        }

        return modelState;
    }
}
