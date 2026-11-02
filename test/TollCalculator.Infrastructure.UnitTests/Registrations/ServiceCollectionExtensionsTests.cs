using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TollCalculator.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using TollCalculator.Application.Providers;
using TollCalculator.Infrastructure.Persistence;
using TollCalculator.Infrastructure.Registrations;
using TollCalculator.Application.Repositories;

namespace TollCalculator.Infrastructure.UnitTests.Registrations;

public class ServiceCollectionExtensionsTests
{
    private const string ConnectionStringKey = "ConnectionStrings:TollDatabase";
    private const string DailyCapKey = "Toll:DailyCap";

    private const string ConnectionString =
        "Server=localhost;Database=TollCalculatorRegistration;Trusted_Connection=True;TrustServerCertificate=True";

    [Theory]
    [InlineData(typeof(TollDbContext))]
    [InlineData(typeof(ITollPassageRepository))]
    [InlineData(typeof(ITollScheduleRepository))]
    [InlineData(typeof(IHolidayProvider))]
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

        var first = firstScope.ServiceProvider.GetRequiredService<ITollPassageRepository>();

        Assert.Same(first, firstScope.ServiceProvider.GetRequiredService<ITollPassageRepository>());
        Assert.NotSame(first, secondScope.ServiceProvider.GetRequiredService<ITollPassageRepository>());
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

        services.AddLogging();
        services.AddInfrastructure(BuildConfiguration(connectionString));

        return services.BuildServiceProvider();
    }

    private static IConfiguration BuildConfiguration(string? connectionString)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [ConnectionStringKey] = connectionString,
                [DailyCapKey] = "60",
            })
            .Build();
    }

    [Theory]
    [InlineData("0")]
    [InlineData(null)]
    public void AddInfrastructure_RejectsADailyCapThatWouldChargeNothing(string? dailyCap)
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddInfrastructure(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [ConnectionStringKey] = ConnectionString,
                [DailyCapKey] = dailyCap,
            })
            .Build());

        using var provider = services.BuildServiceProvider();

        Assert.Throws<OptionsValidationException>(
            () => provider.GetRequiredService<IOptions<TollOptions>>().Value);
    }
}
