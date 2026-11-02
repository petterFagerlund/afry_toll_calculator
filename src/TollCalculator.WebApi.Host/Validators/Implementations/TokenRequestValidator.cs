using Microsoft.AspNetCore.Mvc.ModelBinding;
using TollCalculator.WebApi.Models.V1.Requests;

namespace TollCalculator.WebApi.Host.Validators.Implementations;

internal static class TokenRequestValidator
{
    public static ModelStateDictionary Validate(TokenRequest? request)
    {
        var modelState = new ModelStateDictionary();

        if (request is null)
        {
            modelState.AddModelError(nameof(TokenRequest), "A request body is required.");

            return modelState;
        }

        if (string.IsNullOrWhiteSpace(request.ClientId))
        {
            modelState.AddModelError(nameof(TokenRequest.ClientId), "A client id is required.");
        }

        if (string.IsNullOrWhiteSpace(request.ClientSecret))
        {
            modelState.AddModelError(nameof(TokenRequest.ClientSecret), "A client secret is required.");
        }

        return modelState;
    }
}
