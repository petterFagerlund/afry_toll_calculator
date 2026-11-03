using TollCalculator.Domain.Enums;

namespace TollCalculator.Domain.Constants;

/// <summary>
/// The vehicle types a caller may report on a passage: every <see cref="VehicleType"/> except
/// <see cref="VehicleType.Unknown"/>, which the system assigns itself to an unclassified vehicle.
/// </summary>
public static class ReportableVehicleTypes
{
    public static readonly VehicleType[] All =
        [.. Enum.GetValues<VehicleType>().Where(vehicleType => vehicleType != VehicleType.Unknown)];

    public static string[] Names()
    {
        return Array.ConvertAll(All, vehicleType => vehicleType.ToString());
    }
}
