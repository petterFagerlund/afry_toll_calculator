namespace TollCalculator.WebApi.Host.Auth;

public interface IJwtTokenIssuer
{
    IssuedToken? Issue(string clientId, string clientSecret);
}
