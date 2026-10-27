namespace TollCalculator.WebApi.Models.V1.Requests;

public sealed record RecordTollPassageRequest
{
    public string? RegistrationNumber { get; init; }

    public DateTimeOffset PassedAt { get; init; }
}
