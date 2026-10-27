using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TollCalculator.Infrastructure.Persistence;
using Xunit;

namespace TollCalculator.WebApi.Host.IntegrationTests.Infrastructure;

[Collection(TollApiCollection.Name)]
public class TollApiFixtureTests
{
    private const int DefaultSqlServerPort = 1433;

    private readonly TollApiFixture _fixture;

    public TollApiFixtureTests(TollApiFixture fixture)
    {
        ArgumentNullException.ThrowIfNull(fixture);

        _fixture = fixture;
    }

    [Fact]
    public void TheFixture_RunsAgainstAThrowawayContainerAndNeverTheLocalServer()
    {
        var builder = new SqlConnectionStringBuilder(_fixture.ConnectionString);
        var port = builder.DataSource.Split(',').Last();

        Assert.NotEqual(DefaultSqlServerPort.ToString(), port);
        Assert.NotEqual("TollCalculator", builder.InitialCatalog);
    }

    [Fact]
    public void TheApplicationUnderTest_TalksToTheSameContainerTheFixtureSeeds()
    {
        using var scope = _fixture.Development.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<TollDbContext>();

        var application = new SqlConnectionStringBuilder(context.Database.GetConnectionString()!);
        var fixture = new SqlConnectionStringBuilder(_fixture.ConnectionString);

        Assert.Equal(fixture.DataSource, application.DataSource);
        Assert.Equal(fixture.InitialCatalog, application.InitialCatalog);
    }

    [Fact]
    public async Task ResetTestDataAsync_RemovesTestRowsButKeepsReferenceData()
    {
        await using var connection = new SqlConnection(_fixture.ConnectionString);
        await connection.OpenAsync();

        await ExecuteAsync(connection,
            "INSERT INTO [Vehicles] ([RegistrationNumber], [VehicleType]) VALUES ('RESET01', 1);");

        Assert.Equal(1, await CountAsync(connection, "[Vehicles]"));

        await _fixture.ResetTestDataAsync();

        Assert.Equal(0, await CountAsync(connection, "[Vehicles]"));
        Assert.Equal(9, await CountAsync(connection, "[TollRateIntervals]"));
        Assert.Equal(7, await CountAsync(connection, "[VehicleTypeRules]"));
        Assert.Equal(26, await CountAsync(connection, "[Holidays]"));
    }

    private static async Task ExecuteAsync(SqlConnection connection, string sql)
    {
        await using var command = new SqlCommand(sql, connection);

        await command.ExecuteNonQueryAsync();
    }

    private static async Task<int> CountAsync(SqlConnection connection, string table)
    {
        await using var command = new SqlCommand($"SELECT COUNT(*) FROM {table};", connection);

        return (int)(await command.ExecuteScalarAsync())!;
    }
}
