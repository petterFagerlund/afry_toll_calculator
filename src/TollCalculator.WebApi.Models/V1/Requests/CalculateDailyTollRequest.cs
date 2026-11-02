namespace TollCalculator.WebApi.Models.V1.Requests;

public sealed record CalculateDailyTollRequest
{
    public required string RegistrationNumber { get; init; }

    public required DateOnly Date { get; init; }
}
