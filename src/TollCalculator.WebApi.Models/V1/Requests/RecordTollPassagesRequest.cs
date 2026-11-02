namespace TollCalculator.WebApi.Models.V1.Requests;

public sealed record RecordTollPassagesRequest
{
    public required TollPassageToRecord[] Passages { get; init; }
}

public sealed record TollPassageToRecord
{
    public required string RegistrationNumber { get; init; }

    public required string VehicleType { get; init; }

    public required DateTimeOffset PassedAt { get; init; }
}
