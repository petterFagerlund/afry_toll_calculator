using System.ComponentModel.DataAnnotations;

namespace TollCalculator.Infrastructure.Configuration;

public sealed class TollOptions
{
    public const string SectionName = "Toll";

    // TODO: the daily cap is operational configuration today. Once it varies per environment, move it to a
    // secret store (Key Vault / user-secrets) rather than leaving it in appsettings.json.
    [Range(0, int.MaxValue)]
    public int DailyCap { get; init; }
}
