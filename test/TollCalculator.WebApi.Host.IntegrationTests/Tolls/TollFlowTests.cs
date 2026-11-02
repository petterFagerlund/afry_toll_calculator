using System.Net;
using TollCalculator.Domain.Enums;
using TollCalculator.WebApi.Host.IntegrationTests.Infrastructure;
using TollCalculator.WebApi.Models.V1.Requests;

namespace TollCalculator.WebApi.Host.IntegrationTests.Tolls;

[Collection(TollApiCollection.Name)]
public class TollFlowTests : IAsyncLifetime
{
    private static readonly TimeSpan StockholmWinterOffset = TimeSpan.FromHours(1);
    private static readonly DateOnly Weekday = new(2026, 3, 16);
    private static readonly DateOnly Saturday = new(2026, 3, 21);
    private static readonly DateOnly NewYearsDay = new(2026, 1, 1);

    private readonly TollApiFixture _fixture;
    private readonly TollApiClient _api;
    private readonly HttpClient _client;

    public TollFlowTests(TollApiFixture fixture)
    {
        ArgumentNullException.ThrowIfNull(fixture);

        _fixture = fixture;
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
        using var recorded = await _api.RecordPassageAsync("ABC123", VehicleType.Car, At(Weekday, 7, 15));

        Assert.Equal(HttpStatusCode.Created, recorded.StatusCode);

        var dailyToll = await _api.GetDailyTollAsync("ABC123", Weekday);

        Assert.Equal("ABC123", dailyToll.RegistrationNumber);
        Assert.Equal(Weekday, dailyToll.Date);
        Assert.Equal(18, dailyToll.TotalFee);

        var charged = Assert.Single(dailyToll.ChargedPassages);
        Assert.Equal(At(Weekday, 7, 15), charged.PassedAt);
        Assert.Equal(18, charged.Amount);
    }

    [Fact]
    public async Task ANeverSeenRegistrationNumber_IsRecordedAndCharged()
    {
        using var recorded = await _api.RecordPassageAsync("NEW001", VehicleType.Car, At(Weekday, 7, 15));

        Assert.Equal(HttpStatusCode.Created, recorded.StatusCode);

        var dailyToll = await _api.GetDailyTollAsync("NEW001", Weekday);

        Assert.Equal(18, dailyToll.TotalFee);
    }

    [Fact]
    public async Task OnePostRecordingSeveralVehicles_ChargesEachOnItsOwnType()
    {
        var response = await _api.PostAsync(new RecordTollPassagesRequest
        {
            Passages =
            [
                NewPassage("CAR001", VehicleType.Car, At(Weekday, 7, 15)),
                NewPassage("MC0001", VehicleType.Motorbike, At(Weekday, 7, 15)),
                NewPassage("CAR002", VehicleType.Car, At(Weekday, 15, 40)),
            ],
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        Assert.Equal(18, (await _api.GetDailyTollAsync("CAR001", Weekday)).TotalFee);
        Assert.Equal(0, (await _api.GetDailyTollAsync("MC0001", Weekday)).TotalFee);
        Assert.Equal(18, (await _api.GetDailyTollAsync("CAR002", Weekday)).TotalFee);
    }

    [Fact]
    public async Task ABatchWithOneDuplicate_RecordsNothingAtAll()
    {
        var passedAt = At(Weekday, 7, 15);

        using var first = await _api.RecordPassageAsync("DUP123", VehicleType.Car, passedAt);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var response = await _api.PostAsync(new RecordTollPassagesRequest
        {
            Passages =
            [
                NewPassage("FRESH1", VehicleType.Car, At(Weekday, 9, 0)),
                NewPassage("DUP123", VehicleType.Car, passedAt),
            ],
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(0, (await _api.GetDailyTollAsync("FRESH1", Weekday)).TotalFee);
    }

    [Fact]
    public async Task ABatchWithAPassageMissingItsVehicleType_IsRejectedBeforeAnythingIsRecorded()
    {
        var response = await _api.PostAsync(new RecordTollPassagesRequest
        {
            Passages =
            [
                NewPassage("OK0001", VehicleType.Car, At(Weekday, 7, 15)),
                new TollPassageToRecord
                {
                    RegistrationNumber = "BAD001",
                    VehicleType = "",
                    PassedAt = At(Weekday, 8, 15),
                },
            ],
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, (await _api.GetDailyTollAsync("OK0001", Weekday)).TotalFee);
    }

    [Fact]
    public async Task ABusyDay_IsCappedAtSixtyKronorWithTheLastChargeTrimmed()
    {
        await _api.RecordAndEnsureSuccessAsync("CAP001", VehicleType.Car,
        [
            At(Weekday, 6, 45),
            At(Weekday, 7, 50),
            At(Weekday, 9, 0),
            At(Weekday, 15, 40),
            At(Weekday, 17, 10),
        ]);

        var dailyToll = await _api.GetDailyTollAsync("CAP001", Weekday);

        Assert.Equal(60, dailyToll.TotalFee);
        Assert.Equal([13, 18, 8, 18, 3], dailyToll.ChargedPassages.Select(passage => passage.Amount));
        Assert.Equal(60, dailyToll.ChargedPassages.Sum(passage => passage.Amount));
    }

    [Fact]
    public async Task ATollFreeVehicle_IsNeverCharged()
    {
        await _api.RecordAndEnsureSuccessAsync(
            "MC0001", VehicleType.Motorbike, [At(Weekday, 7, 15), At(Weekday, 15, 40)]);

        var dailyToll = await _api.GetDailyTollAsync("MC0001", Weekday);

        Assert.Equal(0, dailyToll.TotalFee);
        Assert.Empty(dailyToll.ChargedPassages);
    }

    [Fact]
    public async Task APassageOnAPublicHoliday_IsFree()
    {
        await _api.RecordAndEnsureSuccessAsync("HOL001", VehicleType.Car, [At(NewYearsDay, 7, 15)]);

        var dailyToll = await _api.GetDailyTollAsync("HOL001", NewYearsDay);

        Assert.Equal(0, dailyToll.TotalFee);
        Assert.Empty(dailyToll.ChargedPassages);
    }

    [Fact]
    public async Task TwoPassagesInsideOneHour_AreChargedOnceAtTheHigherRate()
    {
        await _api.RecordAndEnsureSuccessAsync(
            "WIN001", VehicleType.Car, [At(Weekday, 6, 20), At(Weekday, 6, 40)]);

        var dailyToll = await _api.GetDailyTollAsync("WIN001", Weekday);

        Assert.Equal(13, dailyToll.TotalFee);

        var charged = Assert.Single(dailyToll.ChargedPassages);
        Assert.Equal(At(Weekday, 6, 40), charged.PassedAt);
        Assert.Equal(13, charged.Amount);
    }

    [Fact]
    public async Task APassageOnASaturday_IsFree()
    {
        await _api.RecordAndEnsureSuccessAsync("SAT001", VehicleType.Car, [At(Saturday, 7, 15)]);

        var dailyToll = await _api.GetDailyTollAsync("SAT001", Saturday);

        Assert.Equal(0, dailyToll.TotalFee);
        Assert.Empty(dailyToll.ChargedPassages);
    }

    [Fact]
    public async Task TheSamePassageRecordedTwice_IsRejectedAsAConflict()
    {
        var passedAt = At(Weekday, 7, 15);

        using var first = await _api.RecordPassageAsync("TWC123", VehicleType.Car, passedAt);
        using var second = await _api.RecordPassageAsync("TWC123", VehicleType.Car, passedAt);

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);

        var dailyToll = await _api.GetDailyTollAsync("TWC123", Weekday);

        Assert.Equal(18, dailyToll.TotalFee);
    }

    [Fact]
    public async Task APassageInTheFuture_IsRejected()
    {
        using var response = await _api.RecordPassageAsync(
            "FUT123", VehicleType.Car, DateTimeOffset.UtcNow.AddDays(1));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task APassageInTheHalfMinuteBetweenTwoRates_IsChargedAtTheEarlierRate()
    {
        var passedAt = At(Weekday, 6, 29).AddSeconds(30);

        using var recorded = await _api.RecordPassageAsync("SEC123", VehicleType.Car, passedAt);

        Assert.Equal(HttpStatusCode.Created, recorded.StatusCode);

        var dailyToll = await _api.GetDailyTollAsync("SEC123", Weekday);

        Assert.Equal(8, dailyToll.TotalFee);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"registrationNumber\":\"ABC123\"}")]
    [InlineData("{\"registrationNumber\":\"ABC123\",\"date\":\"not-a-date\"}")]
    [InlineData("{\"registrationNumber\":\"ABC123\",\"date\":\"2026-13-45\"}")]
    [InlineData("{\"date\":\"2026-03-16\"}")]
    public async Task ATollCalculationWithoutEveryRequiredField_IsRejected(string json)
    {
        using var response = await _api.CalculateRawAsync(json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ADayWithoutPassages_CostsNothing()
    {
        var dailyToll = await _api.GetDailyTollAsync("QUIET1", Weekday);

        Assert.Equal(0, dailyToll.TotalFee);
        Assert.Empty(dailyToll.ChargedPassages);
    }

    private static TollPassageToRecord NewPassage(
        string registrationNumber,
        VehicleType vehicleType,
        DateTimeOffset passedAt)
    {
        return new TollPassageToRecord
        {
            RegistrationNumber = registrationNumber,
            VehicleType = vehicleType.ToString(),
            PassedAt = passedAt,
        };
    }

    private static DateTimeOffset At(DateOnly date, int hour, int minute)
    {
        return new DateTimeOffset(date.Year, date.Month, date.Day, hour, minute, 0, StockholmWinterOffset);
    }
}
