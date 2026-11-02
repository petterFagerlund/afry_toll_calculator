namespace TollCalculator.Domain.Models;

public sealed record TollSettings
{
    private TollSettings(int dailyCap)
    {
        DailyCap = dailyCap;
    }

    public int DailyCap { get; }
}
