using TollCalculator.Domain.Enums;

namespace TollCalculator.Domain.Constants;

/// <summary>
/// The vehicle types a caller may report on a passage: every <see cref="VehicleType"/> except
/// <see cref="VehicleType.Unknown"/>, which the system assigns itself to an unclassified vehicle.
/// </summary>
public static class ReportableVehicleTypes
{
    public static readonly VehicleType[] All = BuildAll();

    public static string[] Names()
    {
        return Array.ConvertAll(All, vehicleType => vehicleType.ToString());
    }

    private static VehicleType[] BuildAll()
    {
        var reportable = new List<VehicleType>();

        foreach (var vehicleType in Enum.GetValues<VehicleType>())
        {
            if (vehicleType != VehicleType.Unknown)
            {
                reportable.Add(vehicleType);
            }
        }

        return [.. reportable];
    }
}
