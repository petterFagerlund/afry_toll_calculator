namespace TollCalculator.WebApi.Models.V1.Responses;

public sealed record CalculateTollResponse
{
    public required string VehicleType { get; init; }

    public required int TotalFee { get; init; }

    public ChargedTollPassage[] ChargedPassages { get; init; } = [];
}
