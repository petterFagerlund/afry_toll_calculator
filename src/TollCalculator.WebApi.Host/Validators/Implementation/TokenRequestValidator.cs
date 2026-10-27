using TollCalculator.WebApi.Models.V1.Requests;

namespace TollCalculator.WebApi.Host.Validators.Implementation;

internal sealed class TokenRequestValidator : ITokenRequestValidator
{
    public ValidationResult Validate(TokenRequest? request)
    {
        var errors = new ValidationErrors();

        if (request is null)
        {
            return errors.Add(nameof(TokenRequest), "A request body is required.").ToResult();
        }

        if (string.IsNullOrWhiteSpace(request.ClientId))
        {
            errors.Add(nameof(TokenRequest.ClientId), "A client id is required.");
        }

        if (string.IsNullOrWhiteSpace(request.ClientSecret))
        {
            errors.Add(nameof(TokenRequest.ClientSecret), "A client secret is required.");
        }

        return errors.ToResult();
    }
}
