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
