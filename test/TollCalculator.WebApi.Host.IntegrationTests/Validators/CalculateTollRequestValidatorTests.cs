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
        var modelState = CalculateTollRequestValidator.Validate(new CalculateTollRequest
        {
            VehicleType = "Car",
            Timestamps = [Morning],
        });

        Assert.True(modelState.IsValid);
        Assert.Empty(modelState);
    }

    [Fact]
    public void Validate_RejectsAMissingRequest()
    {
        var modelState = CalculateTollRequestValidator.Validate(null);

        Assert.False(modelState.IsValid);
        Assert.Contains(nameof(CalculateTollRequest), modelState.Keys);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_RejectsAMissingVehicleType(string? vehicleType)
    {
        var modelState = CalculateTollRequestValidator.Validate(new CalculateTollRequest
        {
            VehicleType = vehicleType,
            Timestamps = [Morning],
        });

        Assert.False(modelState.IsValid);
        Assert.Contains(nameof(CalculateTollRequest.VehicleType), modelState.Keys);
    }

    [Fact]
    public void Validate_AcceptsAnUnknownVehicleTypeAndLeavesItToTheService()
    {
        var modelState = CalculateTollRequestValidator.Validate(new CalculateTollRequest
        {
            VehicleType = "Hovercraft",
            Timestamps = [Morning],
        });

        Assert.True(modelState.IsValid);
    }

    [Fact]
    public void Validate_RejectsAnEmptyTimestampList()
    {
        var modelState = CalculateTollRequestValidator.Validate(
            new CalculateTollRequest { VehicleType = "Car", Timestamps = [] });

        Assert.False(modelState.IsValid);
        Assert.Contains(nameof(CalculateTollRequest.Timestamps), modelState.Keys);
    }

    [Fact]
    public void Validate_RejectsANullTimestampList()
    {
        var modelState = CalculateTollRequestValidator.Validate(
            new CalculateTollRequest { VehicleType = "Car", Timestamps = null! });

        Assert.False(modelState.IsValid);
        Assert.Contains(nameof(CalculateTollRequest.Timestamps), modelState.Keys);
    }

    [Fact]
    public void Validate_ReportsEveryBrokenRuleAtOnce()
    {
        var modelState = CalculateTollRequestValidator.Validate(new CalculateTollRequest());

        Assert.False(modelState.IsValid);
        Assert.Equal(2, modelState.Count);
    }
}
