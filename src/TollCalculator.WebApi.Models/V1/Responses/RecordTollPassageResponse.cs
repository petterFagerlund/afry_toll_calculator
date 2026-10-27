namespace TollCalculator.WebApi.Models.V1.Responses;

public sealed record RecordTollPassageResponse
{
    public required Guid TollPassageId { get; init; }
}
