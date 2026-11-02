using Microsoft.AspNetCore.Mvc.ModelBinding;
using TollCalculator.WebApi.Models.V1.Requests;

namespace TollCalculator.WebApi.Host.Validators.Implementations;

internal static class RecordTollPassageRequestValidator
{
    public static ModelStateDictionary Validate(RecordTollPassageRequest? request, DateTimeOffset now)
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

        var passedAtError = PassageTimestampRule.Check(request.PassedAt, now);

        if (passedAtError is not null)
        {
            modelState.AddModelError(nameof(RecordTollPassageRequest.PassedAt), passedAtError);
        }

        return modelState;
    }
}
