using TollCalculator.Domain.Constants;
using TollCalculator.Domain.Enums;

namespace TollCalculator.Domain.Models;

public sealed record Vehicle
{
    public const string UnregisteredRegistrationNumber = "UNREGISTERED";

    private Vehicle(string registrationNumber, VehicleType vehicleType)
    {
        RegistrationNumber = registrationNumber;
        VehicleType = vehicleType;
    }

    public string RegistrationNumber { get; }

    public VehicleType VehicleType { get; }

    public bool IsTollFree => TollFreeVehicleTypes.Contains(VehicleType);

    public static Vehicle Unregistered(VehicleType vehicleType)
    {
        return new Vehicle(UnregisteredRegistrationNumber, vehicleType);
    }

    public static Vehicle Create(string registrationNumber, VehicleType vehicleType)
    {
        var normalized = RegistrationNumbers.Normalize(registrationNumber, nameof(registrationNumber));

        return new Vehicle(normalized, vehicleType);
    }
}
