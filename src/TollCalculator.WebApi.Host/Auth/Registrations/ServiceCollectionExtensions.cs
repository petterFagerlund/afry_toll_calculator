using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace TollCalculator.WebApi.Host.Auth.Registrations;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBearerAuthentication(
        this IServiceCollection services,
        IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(environment);

        services.AddSingleton<IConfigureOptions<JwtBearerOptions>, JwtBearerOptionsSetup>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

        var fallbackPolicy = environment.IsDevelopment()
            ? AuthorizationPolicies.AnonymousAllowed()
            : AuthorizationPolicies.AuthenticatedUser();

        services.AddAuthorizationBuilder().SetFallbackPolicy(fallbackPolicy);

        return services;
    }
}
