namespace TollCalculator.Domain.Models;

public sealed record TollTariff
{
    private TollTariff(TollSchedule schedule, int dailyCap)
    {
        Schedule = schedule;
        DailyCap = dailyCap;
    }

    public TollSchedule Schedule { get; }

    public int DailyCap { get; }

    public static TollTariff Create(TollSchedule schedule, int dailyCap)
    {
        ArgumentNullException.ThrowIfNull(schedule);

        if (dailyCap < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(dailyCap), dailyCap, "A daily cap must be at least 1; a zero cap charges nothing.");
        }

        return new TollTariff(schedule, dailyCap);
    }
}