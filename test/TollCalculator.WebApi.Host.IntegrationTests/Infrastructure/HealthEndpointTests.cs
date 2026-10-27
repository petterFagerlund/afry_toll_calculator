using System.Net;
using Xunit;

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
        using var client = _fixture.Development.CreateClient();

        var rates = await client.GetAsync("/api/v1/toll-rates");
        var vehicleTypes = await client.GetAsync("/api/v1/vehicle-types");

        Assert.Equal(HttpStatusCode.OK, rates.StatusCode);
        Assert.Equal(HttpStatusCode.OK, vehicleTypes.StatusCode);
    }

    [Fact]
    public async Task ResetTestDataAsync_LeavesTheSeededReferenceDataInPlace()
    {
        await _fixture.ResetTestDataAsync();

        using var client = _fixture.Development.CreateClient();

        var response = await client.GetAsync("/api/v1/holidays");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Nyårsdagen", await response.Content.ReadAsStringAsync());
    }
}
