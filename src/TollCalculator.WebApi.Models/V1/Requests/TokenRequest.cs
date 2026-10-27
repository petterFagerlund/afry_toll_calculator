namespace TollCalculator.WebApi.Models.V1.Requests;

public sealed record TokenRequest
{
    public string? ClientId { get; init; }

    public string? ClientSecret { get; init; }
}
