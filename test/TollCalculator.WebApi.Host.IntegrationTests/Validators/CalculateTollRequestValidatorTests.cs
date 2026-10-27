using TollCalculator.WebApi.Host.Validators.Implementations;
using TollCalculator.WebApi.Models.V1.Requests;
using Xunit;

namespace TollCalculator.WebApi.Host.IntegrationTests.Validators;

public class CalculateTollRequestValidatorTests
{
    private static readonly DateTimeOffset Morning = new(2026, 3, 16, 7, 15, 0, TimeSpan.FromHours(1));

    [Fact]
    public void Validate_AcceptsACompleteRequest()
    {
        var result = CalculateTollRequestValidator.Validate(new CalculateTollRequest
        {
            VehicleType = "Car",
            Timestamps = [Morning],
        });

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_RejectsAMissingRequest()
    {
        var result = CalculateTollRequestValidator.Validate(null);

        Assert.False(result.IsValid);
        Assert.Contains(nameof(CalculateTollRequest), result.Errors.Keys);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_RejectsAMissingVehicleType(string? vehicleType)
    {
        var result = CalculateTollRequestValidator.Validate(new CalculateTollRequest
        {
            VehicleType = vehicleType,
            Timestamps = [Morning],
        });

        Assert.False(result.IsValid);
        Assert.Contains(nameof(CalculateTollRequest.VehicleType), result.Errors.Keys);
    }

    [Fact]
    public void Validate_AcceptsAnUnknownVehicleTypeAndLeavesItToTheService()
    {
        var result = CalculateTollRequestValidator.Validate(new CalculateTollRequest
        {
            VehicleType = "Hovercraft",
            Timestamps = [Morning],
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_RejectsAnEmptyTimestampList()
    {
        var result = CalculateTollRequestValidator.Validate(new CalculateTollRequest { VehicleType = "Car", Timestamps = [] });

        Assert.False(result.IsValid);
        Assert.Contains(nameof(CalculateTollRequest.Timestamps), result.Errors.Keys);
    }

    [Fact]
    public void Validate_RejectsANullTimestampList()
    {
        var result = CalculateTollRequestValidator.Validate(new CalculateTollRequest { VehicleType = "Car", Timestamps = null! });

        Assert.False(result.IsValid);
        Assert.Contains(nameof(CalculateTollRequest.Timestamps), result.Errors.Keys);
    }

    [Fact]
    public void Validate_ReportsEveryBrokenRuleAtOnce()
    {
        var result = CalculateTollRequestValidator.Validate(new CalculateTollRequest());

        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count);
    }
}
