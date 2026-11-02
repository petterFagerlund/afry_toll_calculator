using TollCalculator.Domain.Enums;
using TollCalculator.Domain.Models;
using TollCalculator.WebApi.Models.V1.Responses;
using ContractVehicleType = TollCalculator.WebApi.Models.V1.VehicleType;

namespace TollCalculator.WebApi.Host.Mappers;

internal static class VehicleTypeMapper
{
    public static VehicleTypeResponse[] ToResponses(VehicleTypeRule[] rules)
    {
        ArgumentNullException.ThrowIfNull(rules);

        var responses = new List<VehicleTypeResponse>();

        foreach (var rule in rules)
        {
            responses.Add(new VehicleTypeResponse
            {
                VehicleType = ToContract(rule.VehicleType),
                IsTollFree = rule.IsTollFree,
            });
        }

        return responses.ToArray();
    }

    public static VehicleType ToDomain(ContractVehicleType vehicleType)
    {
        if (!VehicleTypeParser.TryParse(vehicleType.ToString(), out var domainVehicleType))
        {
            throw new ArgumentOutOfRangeException(
                nameof(vehicleType),
                vehicleType,
                "The contract vehicle type has no matching domain vehicle type.");
        }

        return domainVehicleType;
    }

    public static ContractVehicleType ToContract(VehicleType vehicleType)
    {
        if (!Enum.TryParse(vehicleType.ToString(), out ContractVehicleType contractVehicleType) ||
            !Enum.IsDefined(contractVehicleType))
        {
            throw new ArgumentOutOfRangeException(
                nameof(vehicleType),
                vehicleType,
                "The domain vehicle type has no matching contract vehicle type.");
        }

        return contractVehicleType;
    }
}
