using TollCalculator.WebApi.Models.V1.Requests;

namespace TollCalculator.WebApi.Host.Validators.Implementations;

internal static class RecordTollPassageRequestValidator
{
    public static ValidationResult Validate(RecordTollPassageRequest? request)
    {
        var errors = new ValidationErrors();

        if (request is null)
        {
            return errors.Add(nameof(RecordTollPassageRequest), "A request body is required.").ToResult();
        }

        var registrationNumberError = RegistrationNumberRule.Check(request.RegistrationNumber);

        if (registrationNumberError is not null)
        {
            errors.Add(nameof(RecordTollPassageRequest.RegistrationNumber), registrationNumberError);
        }

        if (request.PassedAt == DateTimeOffset.MinValue)
        {
            errors.Add(nameof(RecordTollPassageRequest.PassedAt), "A passage timestamp is required.");
        }

        return errors.ToResult();
    }
}
