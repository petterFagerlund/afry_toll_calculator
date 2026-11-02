namespace TollCalculator.WebApi.Models.V1.Requests;

public sealed record CalculateTollRequest
{
    public VehicleType VehicleType { get; init; }

    public DateTimeOffset[] Timestamps { get; init; } = [];
}
