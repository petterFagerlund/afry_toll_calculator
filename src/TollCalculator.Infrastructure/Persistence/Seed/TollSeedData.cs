namespace TollCalculator.Infrastructure.Persistence.Seed;

internal static class TollSeedData
{
    public static RateIntervalSeed[] RateIntervals { get; } =
    [
        new(1, new TimeOnly(6, 0), new TimeOnly(6, 30), 8),
        new(2, new TimeOnly(6, 30), new TimeOnly(7, 0), 13),
        new(3, new TimeOnly(7, 0), new TimeOnly(8, 0), 18),
        new(4, new TimeOnly(8, 0), new TimeOnly(8, 30), 13),
        new(5, new TimeOnly(8, 30), new TimeOnly(15, 0), 8),
        new(6, new TimeOnly(15, 0), new TimeOnly(15, 30), 13),
        new(7, new TimeOnly(15, 30), new TimeOnly(17, 0), 18),
        new(8, new TimeOnly(17, 0), new TimeOnly(18, 0), 13),
        new(9, new TimeOnly(18, 0), new TimeOnly(18, 30), 8)
    ];
}

internal sealed record RateIntervalSeed(int Id, TimeOnly Start, TimeOnly End, int Amount);
