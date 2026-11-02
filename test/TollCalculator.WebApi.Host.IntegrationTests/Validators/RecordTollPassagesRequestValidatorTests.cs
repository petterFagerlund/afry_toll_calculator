using TollCalculator.Domain.Enums;
using TollCalculator.WebApi.Host.Validators.Implementations;
using TollCalculator.WebApi.Models.V1.Requests;

namespace TollCalculator.WebApi.Host.IntegrationTests.Validators;

public class RecordTollPassagesRequestValidatorTests
{
    private const string RegistrationNumber = "ABC123";

    private static readonly DateTimeOffset PassedAt = new(2026, 3, 16, 7, 15, 0, TimeSpan.FromHours(1));
    private static readonly DateTimeOffset Now = new(2026, 3, 16, 12, 0, 0, TimeSpan.FromHours(1));

    [Fact]
    public void Validate_AcceptsACompleteBatch()
    {
        var modelState = RecordTollPassagesRequestValidator.Validate(
            RequestOf(
                Passage(RegistrationNumber, nameof(VehicleType.Car), PassedAt),
                Passage("XYZ789", nameof(VehicleType.Motorbike), PassedAt)),
            Now);

        Assert.True(modelState.IsValid);
        Assert.Empty(modelState);
    }

    [Fact]
    public void Validate_RejectsAMissingRequest()
    {
        var modelState = RecordTollPassagesRequestValidator.Validate(null, Now);

        Assert.False(modelState.IsValid);
        Assert.Contains("passages", modelState.Keys);
    }

    [Fact]
    public void Validate_RejectsAnEmptyBatch()
    {
        var modelState = RecordTollPassagesRequestValidator.Validate(
            new RecordTollPassagesRequest { Passages = [] }, Now);

        Assert.False(modelState.IsValid);
        Assert.Contains("passages", modelState.Keys);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_RejectsAPassageWithoutAVehicleType(string vehicleType)
    {
        var modelState = RecordTollPassagesRequestValidator.Validate(
            RequestOf(Passage(RegistrationNumber, vehicleType, PassedAt)), Now);

        Assert.False(modelState.IsValid);
        Assert.Contains("passages[0].VehicleType", modelState.Keys);
    }

    [Fact]
    public void Validate_RejectsAVehicleTypeTheDomainDoesNotKnow()
    {
        var modelState = RecordTollPassagesRequestValidator.Validate(
            RequestOf(Passage(RegistrationNumber, "Submarine", PassedAt)), Now);

        Assert.False(modelState.IsValid);
        Assert.Contains("passages[0].VehicleType", modelState.Keys);
    }

    [Fact]
    public void Validate_RejectsUnknownAsAReportedVehicleType()
    {
        var modelState = RecordTollPassagesRequestValidator.Validate(
            RequestOf(Passage(RegistrationNumber, nameof(VehicleType.Unknown), PassedAt)), Now);

        Assert.False(modelState.IsValid);
        Assert.Contains("passages[0].VehicleType", modelState.Keys);
    }

    [Theory]
    [InlineData("Car")]
    [InlineData("car")]
    [InlineData("  MOTORBIKE  ")]
    public void Validate_AcceptsAnyCasingTheDomainCanParse(string vehicleType)
    {
        var modelState = RecordTollPassagesRequestValidator.Validate(
            RequestOf(Passage(RegistrationNumber, vehicleType, PassedAt)), Now);

        Assert.True(modelState.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_RejectsAPassageWithoutARegistrationNumber(string registrationNumber)
    {
        var modelState = RecordTollPassagesRequestValidator.Validate(
            RequestOf(Passage(registrationNumber, nameof(VehicleType.Car), PassedAt)), Now);

        Assert.False(modelState.IsValid);
        Assert.Contains("passages[0].RegistrationNumber", modelState.Keys);
    }

    [Fact]
    public void Validate_RejectsAPassedAtInTheFuture()
    {
        var modelState = RecordTollPassagesRequestValidator.Validate(
            RequestOf(Passage(RegistrationNumber, nameof(VehicleType.Car), Now.AddHours(1))), Now);

        Assert.False(modelState.IsValid);
        Assert.Contains("passages[0].PassedAt", modelState.Keys);
    }

    [Fact]
    public void Validate_RejectsAnUnsetPassedAt()
    {
        var modelState = RecordTollPassagesRequestValidator.Validate(
            RequestOf(new TollPassageToRecord
            {
                RegistrationNumber = RegistrationNumber,
                VehicleType = nameof(VehicleType.Car),
                PassedAt = DateTimeOffset.MinValue,
            }),
            Now);

        Assert.False(modelState.IsValid);
        Assert.Contains("passages[0].PassedAt", modelState.Keys);
    }

    [Fact]
    public void Validate_ReportsTheIndexOfEveryBrokenPassage()
    {
        var modelState = RecordTollPassagesRequestValidator.Validate(
            RequestOf(
                Passage(RegistrationNumber, nameof(VehicleType.Car), PassedAt),
                Passage("", "", PassedAt),
                Passage(RegistrationNumber, "Submarine", PassedAt)),
            Now);

        Assert.False(modelState.IsValid);
        Assert.Contains("passages[1].RegistrationNumber", modelState.Keys);
        Assert.Contains("passages[1].VehicleType", modelState.Keys);
        Assert.Contains("passages[2].VehicleType", modelState.Keys);
        Assert.DoesNotContain("passages[0].VehicleType", modelState.Keys);
    }

    private static TollPassageToRecord Passage(string registrationNumber, string vehicleType, DateTimeOffset passedAt)
    {
        return new TollPassageToRecord
        {
            RegistrationNumber = registrationNumber,
            VehicleType = vehicleType,
            PassedAt = passedAt,
        };
    }

    private static RecordTollPassagesRequest RequestOf(params TollPassageToRecord[] passages)
    {
        return new RecordTollPassagesRequest { Passages = passages };
    }
}
