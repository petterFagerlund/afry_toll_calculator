using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TollCalculator.WebApi.Host.Registrations;
using TollCalculator.WebApi.Models.V1;
using TollCalculator.WebApi.Models.V1.Requests;
using Xunit;

namespace TollCalculator.WebApi.Host.IntegrationTests.Contracts;

public class VehicleTypeSerializationTests
{
    private static readonly DateTimeOffset Morning = new(2026, 3, 16, 7, 15, 0, TimeSpan.FromHours(1));

    private readonly JsonSerializerOptions _options = ConfiguredOptions();

    [Fact]
    public void ARequest_DeserializesTheVehicleTypeFromItsName()
    {
        const string json = """{"vehicleType":"Car","timestamps":["2026-03-16T07:15:00+01:00"]}""";

        var request = JsonSerializer.Deserialize<CalculateTollRequest>(json, _options);

        Assert.Equal(VehicleType.Car, request!.VehicleType);
        Assert.Equal(Morning, Assert.Single(request.Timestamps));
    }

    [Fact]
    public void ARequest_SerializesTheVehicleTypeAsItsNameNotItsNumber()
    {
        var request = new CalculateTollRequest { VehicleType = VehicleType.Motorbike, Timestamps = [Morning] };

        var json = JsonSerializer.Serialize(request, _options);

        Assert.Contains("\"VehicleType\":\"Motorbike\"", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\"VehicleType\":2", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ARequest_WithoutAVehicleType_DeserializesToAValueTheValidatorRejects()
    {
        const string json = """{"timestamps":["2026-03-16T07:15:00+01:00"]}""";

        var request = JsonSerializer.Deserialize<CalculateTollRequest>(json, _options);

        Assert.False(Enum.IsDefined(request!.VehicleType));
    }

    [Theory]
    [InlineData("Hovercraft")]
    [InlineData("Unknown")]
    [InlineData("")]
    public void ARequest_WithAVehicleTypeOutsideTheContract_IsRejectedByTheDeserializer(string vehicleType)
    {
        var json = $$"""{"vehicleType":"{{vehicleType}}","timestamps":["2026-03-16T07:15:00+01:00"]}""";

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<CalculateTollRequest>(json, _options));
    }

    private static JsonSerializerOptions ConfiguredOptions()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddJsonApiControllers();

        using var provider = services.BuildServiceProvider();

        return provider.GetRequiredService<IOptions<JsonOptions>>().Value.JsonSerializerOptions;
    }
}
