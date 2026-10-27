using TollCalculator.WebApi.Models.V1.Requests;

namespace TollCalculator.WebApi.Host.Validators.Implementation;

internal sealed class CalculateTollRequestValidator : ICalculateTollRequestValidator
{
    public ValidationResult Validate(CalculateTollRequest? request)
    {
        var errors = new ValidationErrors();

        if (request is null)
        {
            return errors.Add(nameof(CalculateTollRequest), "A request body is required.").ToResult();
        }

        if (string.IsNullOrWhiteSpace(request.VehicleType))
        {
            errors.Add(nameof(CalculateTollRequest.VehicleType), "A vehicle type is required.");
        }

        if (request.Timestamps is null || request.Timestamps.Length == 0)
        {
            errors.Add(nameof(CalculateTollRequest.Timestamps), "At least one passage timestamp is required.");
        }

        return errors.ToResult();
    }
}
