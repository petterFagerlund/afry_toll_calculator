using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using TollCalculator.WebApi.Host.Configuration;

namespace TollCalculator.WebApi.Host.Auth;

internal sealed class JwtTokenIssuer : IJwtTokenIssuer
{
    private const string BearerTokenType = "Bearer";

    private readonly JwtOptions _jwtOptions;
    private readonly TimeProvider _timeProvider;

    public JwtTokenIssuer(IOptions<JwtOptions> jwtOptions, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(jwtOptions);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _jwtOptions = jwtOptions.Value;
        _timeProvider = timeProvider;
    }

    public IssuedToken? Issue(string clientId, string clientSecret)
    {
        var clientIdMatches = MatchesConstantTime(clientId, _jwtOptions.ClientId);
        var clientSecretMatches = MatchesConstantTime(clientSecret, _jwtOptions.ClientSecret);

        if (!clientIdMatches || !clientSecretMatches)
        {
            return null;
        }

        var issuedAt = _timeProvider.GetUtcNow();
        var expiresAt = issuedAt.AddMinutes(_jwtOptions.TokenLifetimeMinutes);
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
            IssuedAt = issuedAt.UtcDateTime,
            NotBefore = issuedAt.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            Subject = new ClaimsIdentity([new Claim(JwtRegisteredClaimNames.Sub, _jwtOptions.ClientId)]),
            SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256),
        };

        var accessToken = new JsonWebTokenHandler().CreateToken(descriptor);

        return new IssuedToken(accessToken, BearerTokenType, expiresAt);
    }

    private static bool MatchesConstantTime(string? candidate, string expected)
    {
        var candidateBytes = Encoding.UTF8.GetBytes(candidate ?? string.Empty);
        var expectedBytes = Encoding.UTF8.GetBytes(expected);

        return CryptographicOperations.FixedTimeEquals(candidateBytes, expectedBytes);
    }
}
