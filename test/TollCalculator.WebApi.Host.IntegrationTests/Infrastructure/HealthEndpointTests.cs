using System.Net;
using Microsoft.Data.SqlClient;

namespace TollCalculator.WebApi.Host.IntegrationTests.Infrastructure;

[Collection(TollApiCollection.Name)]
public class HealthEndpointTests
{
    private readonly TollApiFixture _fixture;

    public HealthEndpointTests(TollApiFixture fixture)
    {
        ArgumentNullException.ThrowIfNull(fixture);

        _fixture = fixture;
    }

    [Fact]
    public async Task Health_ReportsHealthyAgainstTheContainerDatabase()
    {
        using var client = _fixture.Development.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task TheMigrationCreatedTheSeededReferenceData()
    {
        Assert.Equal(9, await CountAsync("SELECT COUNT(*) FROM [TollRateIntervals];"));
    }

    [Fact]
    public async Task TheMigrationRemovedTheTollSettingsTable()
    {
        var tables = await CountAsync(
            "SELECT COUNT(*) FROM sys.tables WHERE [name] = 'TollSettings';");

        Assert.Equal(0, tables);
    }

    [Fact]
    public async Task TheSeededRateIntervals_LeaveNoGapInTheChargedDay()
    {
        var intervals = await RateIntervalsAsync();

        Assert.Equal(new TimeOnly(6, 0), intervals[0].Start);
        Assert.Equal(new TimeOnly(18, 30), intervals[^1].End);

        for (var i = 1; i < intervals.Length; i++)
        {
            Assert.Equal(intervals[i - 1].End, intervals[i].Start);
        }
    }

    [Fact]
    public async Task ResetTestDataAsync_LeavesTheSeededReferenceDataInPlace()
    {
        await _fixture.ResetTestDataAsync();

        Assert.Equal(9, await CountAsync("SELECT COUNT(*) FROM [TollRateIntervals];"));
    }

    private async Task<(TimeOnly Start, TimeOnly End)[]> RateIntervalsAsync()
    {
        await using var connection = new SqlConnection(_fixture.ConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            "SELECT [Start], [End] FROM [TollRateIntervals] ORDER BY [Start];", connection);

        var intervals = new List<(TimeOnly Start, TimeOnly End)>();

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            intervals.Add((TimeOnly.FromTimeSpan(reader.GetTimeSpan(0)), TimeOnly.FromTimeSpan(reader.GetTimeSpan(1))));
        }

        return intervals.ToArray();
    }

    private async Task<int> CountAsync(string sql)
    {
        await using var connection = new SqlConnection(_fixture.ConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(sql, connection);

        return (int)(await command.ExecuteScalarAsync())!;
    }
}
