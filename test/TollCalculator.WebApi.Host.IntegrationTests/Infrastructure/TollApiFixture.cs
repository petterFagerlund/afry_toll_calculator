using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using TollCalculator.Infrastructure.Registrations;
using Xunit;

namespace TollCalculator.WebApi.Host.IntegrationTests.Infrastructure;

public sealed class TollApiFixture : IAsyncLifetime
{
    private const string ResetTestDataSql =
        """
        DELETE FROM [TollPassages];
        DELETE FROM [Vehicles];
        """;

    private const string SqlServerImage = "mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04";

    private readonly MsSqlContainer _container = new MsSqlBuilder(SqlServerImage).Build();

    private IntegrationTestFactory? _developmentFactory;
    private IntegrationTestFactory? _productionFactory;

    public string ConnectionString
    {
        get { return _container.GetConnectionString(); }
    }

    internal IntegrationTestFactory Development
    {
        get { return _developmentFactory ??= IntegrationTestFactory.Development(ConnectionString); }
    }

    internal IntegrationTestFactory Production
    {
        get { return _productionFactory ??= IntegrationTestFactory.Production(ConnectionString); }
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (_developmentFactory is not null)
        {
            await _developmentFactory.DisposeAsync();
        }

        if (_productionFactory is not null)
        {
            await _productionFactory.DisposeAsync();
        }

        await _container.DisposeAsync();
    }

    public async Task ResetTestDataAsync()
    {
        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(ResetTestDataSql, connection);
        await command.ExecuteNonQueryAsync();
    }

    private async Task MigrateAsync()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"ConnectionStrings:{ServiceCollectionExtensions.ConnectionStringName}"] = ConnectionString,
            })
            .Build();

        var services = new ServiceCollection();
        services.AddInfrastructure(configuration);

        await using var provider = services.BuildServiceProvider();

        await provider.MigrateDatabaseAsync(CancellationToken.None);
    }
}
