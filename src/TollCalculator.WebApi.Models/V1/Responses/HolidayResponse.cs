namespace TollCalculator.WebApi.Models.V1.Responses;

public sealed record HolidayResponse
{
    public required DateOnly Date { get; init; }

    public required string Name { get; init; }
}
