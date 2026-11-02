namespace TollCalculator.WebApi.Models.V1.Responses;

public sealed record VehicleTypeResponse
{
    public required VehicleType VehicleType { get; init; }

    public required bool IsTollFree { get; init; }
}
