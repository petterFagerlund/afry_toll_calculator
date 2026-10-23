namespace TollCalculator.Domain.Models;

public sealed record TollRateInterval
{
    private TollRateInterval(TimeOnly start, TimeOnly end, int amount)
    {
        Start = start;
        End = end;
        Amount = amount;
    }

    public TimeOnly Start { get; }

    public TimeOnly End { get; }

    public int Amount { get; }

    public bool Covers(TimeOnly timeOfDay)
    {
        return timeOfDay >= Start && timeOfDay <= End;
    }

    public static TollRateInterval Create(TimeOnly start, TimeOnly end, int amount)
    {
        if (end < start)
        {
            throw new ArgumentException($"The interval end {end} precedes its start {start}.", nameof(end));
        }

        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "A toll rate cannot be negative.");
        }

        return new TollRateInterval(start, end, amount);
    }
}
