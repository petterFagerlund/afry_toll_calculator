using TollCalculator.Domain.Enums;

namespace TollCalculator.Domain.Models;

public sealed record VehicleTypeRule
{
    private VehicleTypeRule(VehicleType vehicleType, bool isTollFree)
    {
        VehicleType = vehicleType;
        IsTollFree = isTollFree;
    }

    public VehicleType VehicleType { get; }

    public bool IsTollFree { get; }

    public static VehicleTypeRule Create(VehicleType vehicleType, bool isTollFree)
    {
        return new VehicleTypeRule(vehicleType, isTollFree);
    }
}
