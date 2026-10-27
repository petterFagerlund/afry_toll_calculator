namespace TollCalculator.WebApi.Host.Auth;

public static class RateLimitPolicies
{
    public const string TokenIssuance = "token-issuance";

    public const int TokenRequestsPerWindow = 10;

    public static readonly TimeSpan TokenWindow = TimeSpan.FromMinutes(1);
}
