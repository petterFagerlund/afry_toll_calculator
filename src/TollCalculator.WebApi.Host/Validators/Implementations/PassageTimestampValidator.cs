namespace TollCalculator.WebApi.Host.Validators.Implementations;

internal static class PassageTimestampValidator
{
    private static readonly DateTimeOffset EarliestAccepted = new(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly TimeSpan ClockSkew = TimeSpan.FromMinutes(5);

    public static string? Check(DateTimeOffset passedAt, DateTimeOffset now)
    {
        if (passedAt == DateTimeOffset.MinValue || passedAt == DateTimeOffset.MaxValue)
        {
            return "A passage timestamp is required.";
        }

        if (passedAt < EarliestAccepted)
        {
            return $"A passage timestamp cannot be earlier than {EarliestAccepted:yyyy-MM-dd}.";
        }

        if (passedAt > now.Add(ClockSkew))
        {
            return "A passage timestamp cannot be in the future.";
        }

        return null;
    }
}
