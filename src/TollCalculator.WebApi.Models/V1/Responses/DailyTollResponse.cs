namespace TollCalculator.WebApi.Models.V1.Responses;

public sealed record DailyTollResponse
{
    public required string RegistrationNumber { get; init; }

    public required DateOnly Date { get; init; }

    public required int TotalFee { get; init; }

    public ChargedTollPassage[] ChargedPassages { get; init; } = [];
}
