using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TollCalculator.WebApi.Host.Auth.Registrations;
using Xunit;

namespace TollCalculator.WebApi.Host.IntegrationTests.Auth;

public class AuthorizationFallbackPolicyTests
{
    [Fact]
    public async Task OutsideDevelopment_TheFallbackPolicyRequiresAnAuthenticatedUser()
    {
        var policy = await GetFallbackPolicyAsync(Environments.Production);

        Assert.NotNull(policy);
        Assert.Contains(policy.Requirements, requirement => requirement is DenyAnonymousAuthorizationRequirement);
        Assert.Equal([JwtBearerDefaults.AuthenticationScheme], policy.AuthenticationSchemes);
    }

    [Fact]
    public async Task InDevelopment_TheFallbackPolicyAllowsAnonymous()
    {
        var policy = await GetFallbackPolicyAsync(Environments.Development);

        Assert.NotNull(policy);
        Assert.DoesNotContain(policy.Requirements, requirement => requirement is DenyAnonymousAuthorizationRequirement);
    }

    [Theory]
    [InlineData("Development")]
    [InlineData("Production")]
    public void EveryEnvironment_RegistersTheBearerScheme(string environmentName)
    {
        using var application = BuildApplication(environmentName);

        var schemes = application.Services.GetRequiredService<IAuthenticationSchemeProvider>();

        Assert.NotNull(schemes.GetSchemeAsync(JwtBearerDefaults.AuthenticationScheme).GetAwaiter().GetResult());
    }

    private static async Task<AuthorizationPolicy?> GetFallbackPolicyAsync(string environmentName)
    {
        using var application = BuildApplication(environmentName);

        var policyProvider = application.Services.GetRequiredService<IAuthorizationPolicyProvider>();

        return await policyProvider.GetFallbackPolicyAsync();
    }

    private static WebApplication BuildApplication(string environmentName)
    {
        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions
        {
            EnvironmentName = environmentName,
        });

        builder.Services.AddBearerAuthentication(builder.Environment);

        return builder.Build();
    }
}
