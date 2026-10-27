using System.Net.Http.Json;
using System.Text.Json;
using TollCalculator.WebApi.Models.V1.Requests;
using TollCalculator.WebApi.Models.V1.Responses;

namespace TollCalculator.WebApi.Host.IntegrationTests.Tolls;

internal sealed class TollApiClient
{
    private static readonly JsonSerializerOptions SerializerOptions =
        new(JsonSerializerDefaults.Web);

    private readonly HttpClient _client;

    public TollApiClient(HttpClient client)
    {
        ArgumentNullException.ThrowIfNull(client);

        _client = client;
    }

    public async Task<HttpResponseMessage> RecordPassageAsync(string registrationNumber, DateTimeOffset passedAt)
    {
        var request = new RecordTollPassageRequest
        {
            RegistrationNumber = registrationNumber,
            PassedAt = passedAt,
        };

        return await _client.PostAsJsonAsync("/api/v1/toll-passages", request, SerializerOptions);
    }

    public async Task RecordPassagesAsync(string registrationNumber, DateTimeOffset[] passedAt)
    {
        foreach (var moment in passedAt)
        {
            using var response = await RecordPassageAsync(registrationNumber, moment);

            response.EnsureSuccessStatusCode();
        }
    }

    public async Task<DailyTollResponse> GetDailyTollAsync(string registrationNumber, DateOnly date)
    {
        var route = $"/api/v1/vehicles/{registrationNumber}/toll?date={date:yyyy-MM-dd}";

        using var response = await _client.GetAsync(route);

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<DailyTollResponse>(SerializerOptions))!;
    }
}
