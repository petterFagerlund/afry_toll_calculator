using TollCalculator.Domain.Constants;
using TollCalculator.Domain.Enums;
using TollCalculator.Domain.Models;

namespace TollCalculator.Infrastructure.Persistence.Seed;

internal static class TollSeedData
{
    public const int SettingsId = 1;
    public const int DailyCap = 60;

    public static RateIntervalSeed[] RateIntervals { get; } =
    [
        new(1, new TimeOnly(6, 0), new TimeOnly(6, 29), 8),
        new(2, new TimeOnly(6, 30), new TimeOnly(6, 59), 13),
        new(3, new TimeOnly(7, 0), new TimeOnly(7, 59), 18),
        new(4, new TimeOnly(8, 0), new TimeOnly(8, 29), 13),
        new(5, new TimeOnly(8, 30), new TimeOnly(14, 59), 8),
        new(6, new TimeOnly(15, 0), new TimeOnly(15, 29), 13),
        new(7, new TimeOnly(15, 30), new TimeOnly(16, 59), 18),
        new(8, new TimeOnly(17, 0), new TimeOnly(17, 59), 13),
        new(9, new TimeOnly(18, 0), new TimeOnly(18, 29), 8)
    ];

    public static VehicleTypeRule[] VehicleTypeRules { get; } = BuildVehicleTypeRules();

    public static Holiday[] Holidays { get; } =
    [
        Holiday.Create(new DateOnly(2026, 1, 1), "Nyårsdagen"),
        Holiday.Create(new DateOnly(2026, 1, 6), "Trettondedag jul"),
        Holiday.Create(new DateOnly(2026, 4, 3), "Långfredagen"),
        Holiday.Create(new DateOnly(2026, 4, 5), "Påskdagen"),
        Holiday.Create(new DateOnly(2026, 4, 6), "Annandag påsk"),
        Holiday.Create(new DateOnly(2026, 5, 1), "Första maj"),
        Holiday.Create(new DateOnly(2026, 5, 14), "Kristi himmelsfärdsdag"),
        Holiday.Create(new DateOnly(2026, 5, 24), "Pingstdagen"),
        Holiday.Create(new DateOnly(2026, 6, 6), "Sveriges nationaldag"),
        Holiday.Create(new DateOnly(2026, 6, 20), "Midsommardagen"),
        Holiday.Create(new DateOnly(2026, 10, 31), "Alla helgons dag"),
        Holiday.Create(new DateOnly(2026, 12, 25), "Juldagen"),
        Holiday.Create(new DateOnly(2026, 12, 26), "Annandag jul"),
        Holiday.Create(new DateOnly(2027, 1, 1), "Nyårsdagen"),
        Holiday.Create(new DateOnly(2027, 1, 6), "Trettondedag jul"),
        Holiday.Create(new DateOnly(2027, 3, 26), "Långfredagen"),
        Holiday.Create(new DateOnly(2027, 3, 28), "Påskdagen"),
        Holiday.Create(new DateOnly(2027, 3, 29), "Annandag påsk"),
        Holiday.Create(new DateOnly(2027, 5, 1), "Första maj"),
        Holiday.Create(new DateOnly(2027, 5, 6), "Kristi himmelsfärdsdag"),
        Holiday.Create(new DateOnly(2027, 5, 16), "Pingstdagen"),
        Holiday.Create(new DateOnly(2027, 6, 6), "Sveriges nationaldag"),
        Holiday.Create(new DateOnly(2027, 6, 26), "Midsommardagen"),
        Holiday.Create(new DateOnly(2027, 11, 6), "Alla helgons dag"),
        Holiday.Create(new DateOnly(2027, 12, 25), "Juldagen"),
        Holiday.Create(new DateOnly(2027, 12, 26), "Annandag jul")
    ];

    private static VehicleTypeRule[] BuildVehicleTypeRules()
    {
        var vehicleTypes = Enum.GetValues<VehicleType>();
        var rules = new List<VehicleTypeRule>();

        foreach (var vehicleType in vehicleTypes)
        {
            if (vehicleType == VehicleType.Unknown)
            {
                continue;
            }

            rules.Add(VehicleTypeRule.Create(vehicleType, TollFreeVehicleTypes.Contains(vehicleType)));
        }

        return rules.ToArray();
    }
}

internal sealed record RateIntervalSeed(int Id, TimeOnly Start, TimeOnly End, int Amount);
