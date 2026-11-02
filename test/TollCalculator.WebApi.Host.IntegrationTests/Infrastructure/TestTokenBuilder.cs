using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace TollCalculator.WebApi.Host.IntegrationTests.Infrastructure;

internal static class TestTokenBuilder
{
    private const string Subject = "toll-client";

    public static string Create(string issuer, string audience, string signingKey, TimeSpan lifetime)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = issuer,
            Audience = audience,
            Expires = DateTime.UtcNow.Add(lifetime),
            Subject = new ClaimsIdentity([new Claim(JwtRegisteredClaimNames.Sub, Subject)]),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }

    public static string CreateForTheApiUnderTest()
    {
        return Create(
            IntegrationTestFactory.Issuer,
            IntegrationTestFactory.Audience,
            IntegrationTestFactory.SigningKey,
            TimeSpan.FromMinutes(60));
    }
}
