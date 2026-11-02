using System.ComponentModel.DataAnnotations;

namespace TollCalculator.Infrastructure.Configuration;

public sealed class TollOptions
{
    public const string SectionName = "Toll";

    // TODO: the daily cap is operational configuration today. Once it varies per environment, move it to a
    // secret store (Key Vault / user-secrets) rather than leaving it in appsettings.json.
    // The minimum is 1, not 0: a missing "Toll" section binds to 0, and a zero cap charges nothing at all,
    // so startup must fail rather than silently making every passage free.
    [Range(1, int.MaxValue)]
    public int DailyCap { get; init; }
}
