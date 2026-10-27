namespace TollCalculator.WebApi.Host.Auth;

public sealed record IssuedToken(string AccessToken, string TokenType, DateTimeOffset ExpiresAt);
