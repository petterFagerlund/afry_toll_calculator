namespace TollCalculator.WebApi.Host.Validators.Implementations;

internal static class PassageTimestampRule
{
    public static string? Check(DateTimeOffset passedAt)
    {
        if (passedAt == DateTimeOffset.MinValue || passedAt == DateTimeOffset.MaxValue)
        {
            return "A passage timestamp is required.";
        }

        return null;
    }
}
