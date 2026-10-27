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

        if (dailyCap < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(dailyCap), dailyCap, "A daily cap cannot be negative.");
        }

        return new TollTariff(schedule, dailyCap);
    }
}
