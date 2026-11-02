using TollCalculator.Domain.Enums;

namespace TollCalculator.WebApi.Host.Validators.Implementations;

internal static class VehicleTypeRule
{
    public static string? Check(string? vehicleType)
    {
        if (string.IsNullOrWhiteSpace(vehicleType))
        {
            return $"A vehicle type is required; one of {KnownVehicleTypes()}.";
        }

        if (!VehicleTypeParser.TryParse(vehicleType, out _))
        {
            return $"'{vehicleType.Trim()}' is not a known vehicle type; use one of {KnownVehicleTypes()}.";
        }

        return null;
    }

    private static string KnownVehicleTypes()
    {
        var names = new List<string>();

        foreach (var vehicleType in Enum.GetValues<VehicleType>())
        {
            if (vehicleType != VehicleType.Unknown)
            {
                names.Add(vehicleType.ToString());
            }
        }

        return string.Join(", ", names);
    }
}
