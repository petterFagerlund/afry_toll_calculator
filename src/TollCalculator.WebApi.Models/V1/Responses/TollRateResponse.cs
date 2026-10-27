namespace TollCalculator.WebApi.Models.V1.Responses;

public sealed record TollRateResponse
{
    public required string Start { get; init; }

    public required string End { get; init; }

    public required int Amount { get; init; }
}
