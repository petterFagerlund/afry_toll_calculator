using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TollCalculator.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using TollCalculator.Infrastructure.Registrations;
using TollCalculator.WebApi.Host.Configuration;

namespace TollCalculator.WebApi.Host.IntegrationTests.Infrastructure;

internal sealed class IntegrationTestFactory : WebApplicationFactory<Program>
{
    public const string SigningKey = "an-integration-test-signing-key-of-sufficient-length";
    public const string Issuer = "toll-calculator";
    public const string Audience = "toll-calculator-api";
    public const string ClientId = "toll-client";
    public const string ClientSecret = "an-integration-test-client-secret";

    private readonly string _connectionString;
    private readonly string _environmentName;

    public IntegrationTestFactory(string connectionString, string environmentName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentException.ThrowIfNullOrWhiteSpace(environmentName);

        _connectionString = connectionString;
        _environmentName = environmentName;
    }

    public static IntegrationTestFactory Development(string connectionString)
    {
        return new IntegrationTestFactory(connectionString, Environments.Development);
    }

    public static IntegrationTestFactory Production(string connectionString)
    {
        return new IntegrationTestFactory(connectionString, Environments.Production);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.UseEnvironment(_environmentName);

        foreach (var setting in Settings())
        {
            builder.UseSetting(setting.Key, setting.Value);
        }

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<TollDbContext>>();
            services.RemoveAll<DbContextOptions>();
            services.AddDbContext<TollDbContext>(options => options.UseSqlServer(_connectionString));
        });
    }


    private Dictionary<string, string?> Settings()
    {
        var connectionStringKey = $"ConnectionStrings:{ServiceCollectionExtensions.ConnectionStringName}";

        return new Dictionary<string, string?>
        {
            [connectionStringKey] = _connectionString,
            [$"{JwtOptions.SectionName}:SigningKey"] = SigningKey,
            [$"{JwtOptions.SectionName}:Issuer"] = Issuer,
            [$"{JwtOptions.SectionName}:Audience"] = Audience,
            [$"{JwtOptions.SectionName}:TokenLifetimeMinutes"] = "60",
            [$"{JwtOptions.SectionName}:ClientId"] = ClientId,
            [$"{JwtOptions.SectionName}:ClientSecret"] = ClientSecret,
        };
    }
}
