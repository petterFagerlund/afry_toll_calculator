using TollCalculator.WebApi.Models.V1.Requests;

namespace TollCalculator.WebApi.Host.Validators;

public interface ICalculateTollRequestValidator
{
    ValidationResult Validate(CalculateTollRequest? request);
}
