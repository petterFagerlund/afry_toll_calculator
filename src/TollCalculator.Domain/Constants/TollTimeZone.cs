namespace TollCalculator.Domain.Constants;

public static class TollTimeZone
{
    private const string TimeZoneId = "Europe/Stockholm";

    public static TimeZoneInfo Instance { get; } = Load();

    public static DateTimeOffset StartOfDay(DateOnly date)
    {
        var localMidnight = date.ToDateTime(TimeOnly.MinValue);

        return new DateTimeOffset(localMidnight, Instance.GetUtcOffset(localMidnight));
    }

    public static DateTime ToLocalTime(DateTimeOffset moment)
    {
        return TimeZoneInfo.ConvertTime(moment, Instance).DateTime;
    }

    public static DateOnly DateOf(DateTimeOffset moment)
    {
        return DateOnly.FromDateTime(ToLocalTime(moment));
    }

    private static TimeZoneInfo Load()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
        }
        catch (Exception exception) when (
            exception is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            throw new InvalidOperationException(
                $"The time zone '{TimeZoneId}' is not available on this host. Every toll is calculated in " +
                "Stockholm local time, so the host needs a time zone database (install tzdata, or run with " +
                "ICU rather than invariant globalization).",
                exception);
        }
    }
}
