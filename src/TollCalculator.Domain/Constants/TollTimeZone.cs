namespace TollCalculator.Domain.Constants;

public static class TollTimeZone
{
    public static TimeZoneInfo Instance { get; } = TimeZoneInfo.FindSystemTimeZoneById("Europe/Stockholm");

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
}
