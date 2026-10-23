namespace TollCalculator.Domain.Models;

public sealed record ChargedTollPassage(DateTimeOffset PassedAt, int Amount);
