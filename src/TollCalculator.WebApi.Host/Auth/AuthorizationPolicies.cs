using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace TollCalculator.WebApi.Host.Auth;

internal static class AuthorizationPolicies
{
    public static AuthorizationPolicy AuthenticatedUser()
    {
        return new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
            .RequireAuthenticatedUser()
            .Build();
    }

    public static AuthorizationPolicy AnonymousAllowed()
    {
        return new AuthorizationPolicyBuilder()
            .RequireAssertion(_ => true)
            .Build();
    }
}
