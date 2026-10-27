namespace TollCalculator.Domain.Enums;

public static class VehicleTypeParser
{
    public static bool TryParse(string? value, out VehicleType vehicleType)
    {
        vehicleType = VehicleType.Unknown;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (!Enum.TryParse(value.Trim(), ignoreCase: true, out VehicleType parsed))
        {
            return false;
        }

        if (parsed == VehicleType.Unknown)
        {
            return false;
        }

        vehicleType = parsed;

        return true;
    }
}
