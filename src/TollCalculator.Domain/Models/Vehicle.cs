using TollCalculator.Domain.Enums;

namespace TollCalculator.Domain.Models;

public sealed record Vehicle
{
    private Vehicle(string registrationNumber, VehicleType vehicleType)
    {
        RegistrationNumber = registrationNumber;
        VehicleType = vehicleType;
    }

    public string RegistrationNumber { get; }

    public VehicleType VehicleType { get; }

    public static Vehicle Create(string registrationNumber, VehicleType vehicleType)
    {
        var normalized = RegistrationNumbers.Normalize(registrationNumber);

        return new Vehicle(normalized, vehicleType);
    }
}
