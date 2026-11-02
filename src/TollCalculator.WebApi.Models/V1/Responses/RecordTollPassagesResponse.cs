namespace TollCalculator.WebApi.Models.V1.Responses;

public sealed record RecordTollPassagesResponse
{
    public Guid[] TollPassageIds { get; init; } = [];
}
