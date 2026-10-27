namespace TollCalculator.Domain.Constants;

public static class TollTimeZone
{
    public static TimeZoneInfo Instance { get; } = TimeZoneInfo.FindSystemTimeZoneById("Europe/Stockholm");

    public static DateTimeOffset StartOfDay(DateOnly date)
    {
        var localMidnight = date.ToDateTime(TimeOnly.MinValue);

        return new DateTimeOffset(localMidnight, Instance.GetUtcOffset(localMidnight));
    }

    public static DateOnly DateOf(DateTimeOffset moment)
    {
        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(moment, Instance).DateTime);
    }
}
