namespace TollCalculator.Domain.Models;

public sealed record DailyToll(int TotalFee, ChargedTollPassage[] ChargedPassages)
{
    public static DailyToll None()
    {
        return new DailyToll(0, []);
    }
}
