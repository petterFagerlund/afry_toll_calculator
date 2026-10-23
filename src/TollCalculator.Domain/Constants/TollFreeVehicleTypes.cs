using TollCalculator.Domain.Enums;

namespace TollCalculator.Domain.Constants;

public static class TollFreeVehicleTypes
{
    public static readonly VehicleType[] All =
    [
        VehicleType.Motorbike,
        VehicleType.Tractor,
        VehicleType.Emergency,
        VehicleType.Diplomat,
        VehicleType.Foreign,
        VehicleType.Military
    ];

    public static bool Contains(VehicleType vehicleType)
    {
        return Array.IndexOf(All, vehicleType) >= 0;
    }
}
