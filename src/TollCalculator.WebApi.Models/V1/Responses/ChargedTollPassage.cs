namespace TollCalculator.WebApi.Models.V1.Responses;

public sealed record ChargedTollPassage
{
    public required DateTimeOffset PassedAt { get; init; }

    public required int Amount { get; init; }
}
