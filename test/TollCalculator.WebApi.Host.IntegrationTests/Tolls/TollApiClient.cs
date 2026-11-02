using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using TollCalculator.Domain.Enums;
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

    public async Task<HttpResponseMessage> RecordPassageAsync(
        string registrationNumber,
        VehicleType vehicleType,
        DateTimeOffset passedAt)
    {
        return await RecordPassagesAsync(registrationNumber, vehicleType, [passedAt]);
    }

    public async Task<HttpResponseMessage> RecordPassagesAsync(
        string registrationNumber,
        VehicleType vehicleType,
        DateTimeOffset[] passedAt)
    {
        var passages = Array.ConvertAll(passedAt, moment => new TollPassageToRecord
        {
            RegistrationNumber = registrationNumber,
            VehicleType = vehicleType.ToString(),
            PassedAt = moment,
        });

        return await PostAsync(new RecordTollPassagesRequest { Passages = passages });
    }

    public async Task<HttpResponseMessage> PostAsync(RecordTollPassagesRequest request)
    {
        return await _client.PostAsJsonAsync("/api/v1/toll-passages", request, SerializerOptions);
    }

    public async Task<RecordTollPassagesResponse> RecordAndEnsureSuccessAsync(
        string registrationNumber,
        VehicleType vehicleType,
        DateTimeOffset[] passedAt)
    {
        using var response = await RecordPassagesAsync(registrationNumber, vehicleType, passedAt);

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<RecordTollPassagesResponse>(SerializerOptions))!;
    }

    public async Task<DailyTollResponse> GetDailyTollAsync(string registrationNumber, DateOnly date)
    {
        using var response = await CalculateAsync(
            new CalculateDailyTollRequest { RegistrationNumber = registrationNumber, Date = date });

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<DailyTollResponse>(SerializerOptions))!;
    }

    public async Task<HttpResponseMessage> CalculateAsync(CalculateDailyTollRequest request)
    {
        return await _client.PostAsJsonAsync("/api/v1/toll-calculations", request, SerializerOptions);
    }

    public async Task<HttpResponseMessage> CalculateRawAsync(string json)
    {
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        return await _client.PostAsync("/api/v1/toll-calculations", content);
    }
}
