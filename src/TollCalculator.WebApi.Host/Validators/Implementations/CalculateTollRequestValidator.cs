using Microsoft.AspNetCore.Mvc.ModelBinding;
using TollCalculator.WebApi.Models.V1;
using TollCalculator.WebApi.Models.V1.Requests;

namespace TollCalculator.WebApi.Host.Validators.Implementations;

internal static class CalculateTollRequestValidator
{
    private static readonly string KnownVehicleTypes = string.Join(", ", Enum.GetNames<VehicleType>());

    public static ModelStateDictionary Validate(CalculateTollRequest? request)
    {
        var modelState = new ModelStateDictionary();

        if (request is null)
        {
            modelState.AddModelError(nameof(CalculateTollRequest), "A request body is required.");

            return modelState;
        }

        if (!Enum.IsDefined(request.VehicleType))
        {
            modelState.AddModelError(
                nameof(CalculateTollRequest.VehicleType),
                $"A vehicle type is required and must be one of: {KnownVehicleTypes}.");
        }

        if (request.Timestamps is null || request.Timestamps.Length == 0)
        {
            modelState.AddModelError(
                nameof(CalculateTollRequest.Timestamps),
                "At least one passage timestamp is required.");

            return modelState;
        }

        for (var index = 0; index < request.Timestamps.Length; index++)
        {
            var timestampError = PassageTimestampRule.Check(request.Timestamps[index]);

            if (timestampError is not null)
            {
                modelState.AddModelError($"{nameof(CalculateTollRequest.Timestamps)}[{index}]", timestampError);
            }
        }

        return modelState;
    }
}
