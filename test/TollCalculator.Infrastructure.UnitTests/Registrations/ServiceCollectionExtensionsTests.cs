using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TollCalculator.Infrastructure.Persistence;
using TollCalculator.Infrastructure.Registrations;
using TollCalculator.Infrastructure.Repositories;
using Xunit;

namespace TollCalculator.Infrastructure.UnitTests.Registrations;

public class ServiceCollectionExtensionsTests
{
    private const string ConnectionStringKey = "ConnectionStrings:TollDatabase";

    private const string ConnectionString =
        "Server=localhost;Database=TollCalculatorRegistration;Trusted_Connection=True;TrustServerCertificate=True";

    [Theory]
    [InlineData(typeof(TollDbContext))]
    [InlineData(typeof(IVehicleRepository))]
    [InlineData(typeof(ITollPassageRepository))]
    [InlineData(typeof(ITollScheduleRepository))]
    [InlineData(typeof(IHolidayRepository))]
    [InlineData(typeof(IVehicleTypeRuleRepository))]
    public void AddInfrastructure_ResolvesRegisteredService(Type serviceType)
    {
        using var provider = BuildProvider(ConnectionString);
        using var scope = provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetRequiredService(serviceType));
    }

    [Fact]
    public void AddInfrastructure_RegistersRepositoriesAsScoped()
    {
        using var provider = BuildProvider(ConnectionString);

        using var firstScope = provider.CreateScope();
        using var secondScope = provider.CreateScope();

        var first = firstScope.ServiceProvider.GetRequiredService<IVehicleRepository>();

        Assert.Same(first, firstScope.ServiceProvider.GetRequiredService<IVehicleRepository>());
        Assert.NotSame(first, secondScope.ServiceProvider.GetRequiredService<IVehicleRepository>());
    }

    [Fact]
    public void AddInfrastructure_UsesTheConfiguredSqlServerConnectionString()
    {
        using var provider = BuildProvider(ConnectionString);
        using var scope = provider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<TollDbContext>();

        Assert.True(context.Database.IsSqlServer());
        Assert.Equal("TollCalculatorRegistration", context.Database.GetDbConnection().Database);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddInfrastructure_ThrowsWhenConnectionStringIsMissing(string? connectionString)
    {
        var services = new ServiceCollection();

        Assert.Throws<InvalidOperationException>(() => services.AddInfrastructure(BuildConfiguration(connectionString)));
    }

    private static ServiceProvider BuildProvider(string connectionString)
    {
        var services = new ServiceCollection();

        services.AddInfrastructure(BuildConfiguration(connectionString));

        return services.BuildServiceProvider();
    }

    private static IConfiguration BuildConfiguration(string? connectionString)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { [ConnectionStringKey] = connectionString })
            .Build();
    }
}
