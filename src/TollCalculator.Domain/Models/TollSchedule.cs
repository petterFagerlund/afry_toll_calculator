namespace TollCalculator.Domain.Models;

public sealed class TollSchedule
{
    private readonly TollRateInterval[] _intervals;

    private TollSchedule(TollRateInterval[] intervals)
    {
        _intervals = intervals;
    }

    public IReadOnlyList<TollRateInterval> Intervals => _intervals;

    public int FeeFor(TimeOnly timeOfDay)
    {
        var coveringInterval = _intervals.FirstOrDefault(interval => interval.Covers(timeOfDay));
        if (coveringInterval is null)
        {
            return 0;
        }

        return coveringInterval.Amount;
    }

    public static TollSchedule Create(TollRateInterval[] intervals)
    {
        ArgumentNullException.ThrowIfNull(intervals);

        var ordered = intervals.OrderBy(interval => interval.Start).ToArray();

        for (var index = 1; index < ordered.Length; index++)
        {
            if (ordered[index].Start < ordered[index - 1].End)
            {
                throw new ArgumentException(
                    $"The interval starting {ordered[index].Start} overlaps the one ending {ordered[index - 1].End}.",
                    nameof(intervals));
            }
        }

        return new TollSchedule(ordered);
    }
}
