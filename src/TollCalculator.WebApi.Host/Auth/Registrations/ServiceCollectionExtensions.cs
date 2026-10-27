using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using System.Threading.RateLimiting;

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

        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<IJwtTokenIssuer, JwtTokenIssuer>();

        return services;
    }

    public static IServiceCollection AddTokenRateLimiting(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddFixedWindowLimiter(RateLimitPolicies.TokenIssuance, limiter =>
            {
                limiter.PermitLimit = RateLimitPolicies.TokenRequestsPerWindow;
                limiter.Window = RateLimitPolicies.TokenWindow;
                limiter.QueueLimit = 0;
                limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });
        });

        return services;
    }
}
