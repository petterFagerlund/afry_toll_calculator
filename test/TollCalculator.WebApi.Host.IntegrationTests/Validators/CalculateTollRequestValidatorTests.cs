using TollCalculator.WebApi.Host.Validators.Implementations;
using TollCalculator.WebApi.Models.V1;
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
            VehicleType = VehicleType.Car,
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

    [Fact]
    public void Validate_RejectsAnOmittedVehicleType()
    {
        var modelState = CalculateTollRequestValidator.Validate(new CalculateTollRequest
        {
            Timestamps = [Morning],
        });

        Assert.False(modelState.IsValid);
        Assert.Contains(nameof(CalculateTollRequest.VehicleType), modelState.Keys);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(99)]
    [InlineData(-1)]
    public void Validate_RejectsAVehicleTypeOutsideTheContract(int vehicleType)
    {
        var modelState = CalculateTollRequestValidator.Validate(new CalculateTollRequest
        {
            VehicleType = (VehicleType)vehicleType,
            Timestamps = [Morning],
        });

        Assert.False(modelState.IsValid);
        Assert.Contains(nameof(CalculateTollRequest.VehicleType), modelState.Keys);
    }

    [Fact]
    public void Validate_RejectingAVehicleType_NamesTheKnownTypes()
    {
        var modelState = CalculateTollRequestValidator.Validate(new CalculateTollRequest
        {
            Timestamps = [Morning],
        });

        var message = Assert.Single(modelState[nameof(CalculateTollRequest.VehicleType)]!.Errors).ErrorMessage;

        Assert.Contains(nameof(VehicleType.Car), message, StringComparison.Ordinal);
        Assert.Contains(nameof(VehicleType.Military), message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(VehicleType.Car)]
    [InlineData(VehicleType.Motorbike)]
    [InlineData(VehicleType.Military)]
    public void Validate_AcceptsEveryContractVehicleType(VehicleType vehicleType)
    {
        var modelState = CalculateTollRequestValidator.Validate(new CalculateTollRequest
        {
            VehicleType = vehicleType,
            Timestamps = [Morning],
        });

        Assert.True(modelState.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void Validate_RejectsADefaultTimestamp_KeyedOnItsIndex(int index)
    {
        var timestamps = new[] { Morning, Morning.AddHours(2), Morning.AddHours(4) };
        timestamps[index] = DateTimeOffset.MinValue;

        var modelState = CalculateTollRequestValidator.Validate(new CalculateTollRequest
        {
            VehicleType = VehicleType.Car,
            Timestamps = timestamps,
        });

        Assert.False(modelState.IsValid);
        Assert.Contains($"{nameof(CalculateTollRequest.Timestamps)}[{index}]", modelState.Keys);
    }

    [Fact]
    public void Validate_RejectsAMaximumTimestamp()
    {
        var modelState = CalculateTollRequestValidator.Validate(new CalculateTollRequest
        {
            VehicleType = VehicleType.Car,
            Timestamps = [DateTimeOffset.MaxValue],
        });

        Assert.False(modelState.IsValid);
        Assert.Contains($"{nameof(CalculateTollRequest.Timestamps)}[0]", modelState.Keys);
    }

    [Fact]
    public void Validate_RejectsEveryDefaultTimestampNotJustTheFirst()
    {
        var modelState = CalculateTollRequestValidator.Validate(new CalculateTollRequest
        {
            VehicleType = VehicleType.Car,
            Timestamps = [DateTimeOffset.MinValue, Morning, DateTimeOffset.MinValue],
        });

        Assert.Contains($"{nameof(CalculateTollRequest.Timestamps)}[0]", modelState.Keys);
        Assert.DoesNotContain($"{nameof(CalculateTollRequest.Timestamps)}[1]", modelState.Keys);
        Assert.Contains($"{nameof(CalculateTollRequest.Timestamps)}[2]", modelState.Keys);
    }

    [Fact]
    public void Validate_AcceptsTimestampsAcrossDifferentOffsets()
    {
        var modelState = CalculateTollRequestValidator.Validate(new CalculateTollRequest
        {
            VehicleType = VehicleType.Car,
            Timestamps = [Morning, Morning.ToUniversalTime(), Morning.ToOffset(TimeSpan.FromHours(-5))],
        });

        Assert.True(modelState.IsValid);
    }

    [Fact]
    public void Validate_RejectsAnEmptyTimestampList()
    {
        var modelState = CalculateTollRequestValidator.Validate(
            new CalculateTollRequest { VehicleType = VehicleType.Car, Timestamps = [] });

        Assert.False(modelState.IsValid);
        Assert.Contains(nameof(CalculateTollRequest.Timestamps), modelState.Keys);
    }

    [Fact]
    public void Validate_RejectsANullTimestampList()
    {
        var modelState = CalculateTollRequestValidator.Validate(
            new CalculateTollRequest { VehicleType = VehicleType.Car, Timestamps = null! });

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
