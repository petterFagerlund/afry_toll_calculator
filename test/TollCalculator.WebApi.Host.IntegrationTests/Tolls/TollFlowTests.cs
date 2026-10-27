using System.Net;
using TollCalculator.Domain.Enums;
using TollCalculator.WebApi.Host.IntegrationTests.Infrastructure;
using Xunit;

namespace TollCalculator.WebApi.Host.IntegrationTests.Tolls;

[Collection(TollApiCollection.Name)]
public class TollFlowTests : IAsyncLifetime
{
    private static readonly TimeSpan StockholmWinterOffset = TimeSpan.FromHours(1);
    private static readonly DateOnly Weekday = new(2026, 3, 16);
    private static readonly DateOnly Saturday = new(2026, 3, 21);
    private static readonly DateOnly NewYearsDay = new(2026, 1, 1);

    private readonly TollApiFixture _fixture;
    private readonly TestVehicleSeeder _vehicles;
    private readonly TollApiClient _api;
    private readonly HttpClient _client;

    public TollFlowTests(TollApiFixture fixture)
    {
        ArgumentNullException.ThrowIfNull(fixture);

        _fixture = fixture;
        _vehicles = new TestVehicleSeeder(fixture.ConnectionString);
        _client = fixture.Development.CreateClient();
        _api = new TollApiClient(_client);
    }

    public async Task InitializeAsync()
    {
        await _fixture.ResetTestDataAsync();
    }

    public Task DisposeAsync()
    {
        _client.Dispose();

        return Task.CompletedTask;
    }

    [Fact]
    public async Task ARecordedPassage_IsChargedAtTheRateForItsTimeOfDay()
    {
        var registrationNumber = await _vehicles.AddAsync("ABC123", VehicleType.Car);

        using var recorded = await _api.RecordPassageAsync(registrationNumber, At(Weekday, 7, 15));

        Assert.Equal(HttpStatusCode.Created, recorded.StatusCode);

        var dailyToll = await _api.GetDailyTollAsync(registrationNumber, Weekday);

        Assert.Equal(registrationNumber, dailyToll.RegistrationNumber);
        Assert.Equal(Weekday, dailyToll.Date);
        Assert.Equal(18, dailyToll.TotalFee);

        var charged = Assert.Single(dailyToll.ChargedPassages);
        Assert.Equal(At(Weekday, 7, 15), charged.PassedAt);
        Assert.Equal(18, charged.Amount);
    }

    [Fact]
    public async Task ABusyDay_IsCappedAtSixtyKronorWithTheLastChargeTrimmed()
    {
        var registrationNumber = await _vehicles.AddAsync("CAP001", VehicleType.Car);

        await _api.RecordPassagesAsync(registrationNumber,
        [
            At(Weekday, 6, 45),
            At(Weekday, 7, 50),
            At(Weekday, 9, 0),
            At(Weekday, 15, 40),
            At(Weekday, 17, 10),
        ]);

        var dailyToll = await _api.GetDailyTollAsync(registrationNumber, Weekday);

        Assert.Equal(60, dailyToll.TotalFee);
        Assert.Equal([13, 18, 8, 18, 3], dailyToll.ChargedPassages.Select(passage => passage.Amount));
        Assert.Equal(60, dailyToll.ChargedPassages.Sum(passage => passage.Amount));
    }

    [Fact]
    public async Task ATollFreeVehicle_IsNeverCharged()
    {
        var registrationNumber = await _vehicles.AddAsync("MC0001", VehicleType.Motorbike);

        await _api.RecordPassagesAsync(registrationNumber, [At(Weekday, 7, 15), At(Weekday, 15, 40)]);

        var dailyToll = await _api.GetDailyTollAsync(registrationNumber, Weekday);

        Assert.Equal(0, dailyToll.TotalFee);
        Assert.Empty(dailyToll.ChargedPassages);
    }

    [Fact]
    public async Task APassageOnASeededHoliday_IsFree()
    {
        var registrationNumber = await _vehicles.AddAsync("HOL001", VehicleType.Car);

        await _api.RecordPassagesAsync(registrationNumber, [At(NewYearsDay, 7, 15)]);

        var dailyToll = await _api.GetDailyTollAsync(registrationNumber, NewYearsDay);

        Assert.Equal(0, dailyToll.TotalFee);
        Assert.Empty(dailyToll.ChargedPassages);
    }

    [Fact]
    public async Task TwoPassagesInsideOneHour_AreChargedOnceAtTheHigherRate()
    {
        var registrationNumber = await _vehicles.AddAsync("WIN001", VehicleType.Car);

        await _api.RecordPassagesAsync(registrationNumber, [At(Weekday, 6, 20), At(Weekday, 6, 40)]);

        var dailyToll = await _api.GetDailyTollAsync(registrationNumber, Weekday);

        Assert.Equal(13, dailyToll.TotalFee);

        var charged = Assert.Single(dailyToll.ChargedPassages);
        Assert.Equal(At(Weekday, 6, 40), charged.PassedAt);
        Assert.Equal(13, charged.Amount);
    }

    [Fact]
    public async Task APassageOnASaturday_IsFree()
    {
        var registrationNumber = await _vehicles.AddAsync("SAT001", VehicleType.Car);

        await _api.RecordPassagesAsync(registrationNumber, [At(Saturday, 7, 15)]);

        var dailyToll = await _api.GetDailyTollAsync(registrationNumber, Saturday);

        Assert.Equal(0, dailyToll.TotalFee);
        Assert.Empty(dailyToll.ChargedPassages);
    }

    [Fact]
    public async Task APassageForAnUnregisteredVehicle_IsRejected()
    {
        using var response = await _api.RecordPassageAsync("NOPE01", At(Weekday, 7, 15));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ADayWithoutPassages_CostsNothing()
    {
        var registrationNumber = await _vehicles.AddAsync("QUIET1", VehicleType.Car);

        var dailyToll = await _api.GetDailyTollAsync(registrationNumber, Weekday);

        Assert.Equal(0, dailyToll.TotalFee);
        Assert.Empty(dailyToll.ChargedPassages);
    }

    private static DateTimeOffset At(DateOnly date, int hour, int minute)
    {
        return new DateTimeOffset(date.Year, date.Month, date.Day, hour, minute, 0, StockholmWinterOffset);
    }
}
