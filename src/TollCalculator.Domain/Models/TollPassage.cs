using TollCalculator.Domain.Enums;

namespace TollCalculator.Domain.Models;

public sealed record TollPassage
{
    private TollPassage(Guid id, string registrationNumber, VehicleType vehicleType, DateTimeOffset passedAt)
    {
        Id = id;
        RegistrationNumber = registrationNumber;
        VehicleType = vehicleType;
        PassedAt = passedAt;
    }

    public Guid Id { get; }

    public string RegistrationNumber { get; }

    public VehicleType VehicleType { get; }

    public DateTimeOffset PassedAt { get; }

    public static TollPassage Create(string registrationNumber, VehicleType vehicleType, DateTimeOffset passedAt)
    {
        var normalized = RegistrationNumbers.Normalize(registrationNumber);

        return new TollPassage(Guid.CreateVersion7(), normalized, vehicleType, passedAt);
    }
}
