namespace TollCalculator.Domain.Models;

public sealed record TollPassage
{
    private TollPassage(Guid id, string registrationNumber, DateTimeOffset passedAt)
    {
        Id = id;
        RegistrationNumber = registrationNumber;
        PassedAt = passedAt;
    }

    public Guid Id { get; }

    public string RegistrationNumber { get; }

    public DateTimeOffset PassedAt { get; }

    public static TollPassage Create(string registrationNumber, DateTimeOffset passedAt)
    {
        var normalized = RegistrationNumbers.Normalize(registrationNumber, nameof(registrationNumber));

        return new TollPassage(Guid.CreateVersion7(), normalized, passedAt);
    }
}
