namespace TollCalculator.WebApi.Host.Validators;

internal sealed class ValidationErrors
{
    private readonly Dictionary<string, string[]> _errors = [];

    public ValidationErrors Add(string memberName, string message)
    {
        _errors[memberName] = [message];

        return this;
    }

    public ValidationResult ToResult()
    {
        if (_errors.Count == 0)
        {
            return ValidationResult.Valid();
        }

        return ValidationResult.Failed(_errors);
    }
}
