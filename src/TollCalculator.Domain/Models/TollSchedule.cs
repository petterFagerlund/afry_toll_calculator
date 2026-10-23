namespace TollCalculator.Domain.Models;

public sealed class TollSchedule
{
    private readonly TollRateInterval[] _intervals;

    private TollSchedule(TollRateInterval[] intervals)
    {
        _intervals = intervals;
    }

    public IReadOnlyList<TollRateInterval> Intervals
    {
        get { return _intervals; }
    }

    public int FeeFor(TimeOnly timeOfDay)
    {
        foreach (var interval in _intervals)
        {
            if (interval.Covers(timeOfDay))
            {
                return interval.Amount;
            }
        }

        return 0;
    }

    public static TollSchedule Create(TollRateInterval[] intervals)
    {
        ArgumentNullException.ThrowIfNull(intervals);

        var ordered = new TollRateInterval[intervals.Length];
        Array.Copy(intervals, ordered, intervals.Length);
        Array.Sort(ordered, (left, right) => left.Start.CompareTo(right.Start));

        for (var i = 1; i < ordered.Length; i++)
        {
            if (ordered[i].Start <= ordered[i - 1].End)
            {
                throw new ArgumentException(
                    $"The interval starting {ordered[i].Start} overlaps the one ending {ordered[i - 1].End}.",
                    nameof(intervals));
            }
        }

        return new TollSchedule(ordered);
    }
}
