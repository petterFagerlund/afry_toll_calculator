using Microsoft.AspNetCore.Mvc.ModelBinding;
using TollCalculator.WebApi.Models.V1.Requests;

namespace TollCalculator.WebApi.Host.Validators.Implementations;

internal static class CalculateTollRequestValidator
{
    public static ModelStateDictionary Validate(CalculateTollRequest? request)
    {
        var modelState = new ModelStateDictionary();

        if (request is null)
        {
            modelState.AddModelError(nameof(CalculateTollRequest), "A request body is required.");

            return modelState;
        }

        if (string.IsNullOrWhiteSpace(request.VehicleType))
        {
            modelState.AddModelError(nameof(CalculateTollRequest.VehicleType), "A vehicle type is required.");
        }

        if (request.Timestamps is null || request.Timestamps.Length == 0)
        {
            modelState.AddModelError(
                nameof(CalculateTollRequest.Timestamps),
                "At least one passage timestamp is required.");
        }

        return modelState;
    }
}
