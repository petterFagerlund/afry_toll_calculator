using TollCalculator.Domain.Constants;
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

    public bool IsTollFree => TollFreeVehicleTypes.Contains(VehicleType);

    public static Vehicle Create(string registrationNumber, VehicleType vehicleType)
    {
        if (string.IsNullOrWhiteSpace(registrationNumber))
        {
            throw new ArgumentException("A registration number is required.", nameof(registrationNumber));
        }

        return new Vehicle(registrationNumber.Trim().ToUpperInvariant(), vehicleType);
    }
}
