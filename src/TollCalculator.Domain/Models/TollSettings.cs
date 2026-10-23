namespace TollCalculator.Domain.Models;

public sealed record TollSettings
{
    private TollSettings(int dailyCap)
    {
        DailyCap = dailyCap;
    }

    public int DailyCap { get; }

    public static TollSettings Create(int dailyCap)
    {
        if (dailyCap < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(dailyCap), dailyCap, "A daily cap cannot be negative.");
        }

        return new TollSettings(dailyCap);
    }
}
