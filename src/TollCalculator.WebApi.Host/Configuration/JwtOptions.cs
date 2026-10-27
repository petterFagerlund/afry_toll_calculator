using System.ComponentModel.DataAnnotations;

namespace TollCalculator.WebApi.Host.Configuration;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public const int MinimumSigningKeyLength = 32;

    [Required]
    [MinLength(MinimumSigningKeyLength)]
    public string SigningKey { get; init; } = string.Empty;

    [Required]
    public string Issuer { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;

    [Range(1, 1440)]
    public int TokenLifetimeMinutes { get; init; } = 60;
}
