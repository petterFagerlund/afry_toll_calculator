namespace TollCalculator.WebApi.Host.Validators;

public sealed record ValidationResult
{
    private static readonly IReadOnlyDictionary<string, string[]> NoErrors =
        new Dictionary<string, string[]>();

    private ValidationResult(IReadOnlyDictionary<string, string[]> errors)
    {
        Errors = errors;
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public bool IsValid
    {
        get { return Errors.Count == 0; }
    }

    public static ValidationResult Valid()
    {
        return new ValidationResult(NoErrors);
    }

    public static ValidationResult Failed(IReadOnlyDictionary<string, string[]> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        return new ValidationResult(errors);
    }
}
