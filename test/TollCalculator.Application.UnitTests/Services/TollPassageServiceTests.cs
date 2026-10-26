using NSubstitute;
using TollCalculator.Application.Services.Implementations;
using TollCalculator.Domain.Enums;
using TollCalculator.Domain.Models;
using TollCalculator.Infrastructure.Repositories;
using Xunit;

namespace TollCalculator.Application.UnitTests.Services;

public class TollPassageServiceTests
{
    private static readonly DateTimeOffset PassedAt = new(2026, 3, 16, 7, 15, 0, TimeSpan.FromHours(1));

    private readonly IVehicleRepository _vehicleRepository = Substitute.For<IVehicleRepository>();
    private readonly ITollPassageRepository _tollPassageRepository = Substitute.For<ITollPassageRepository>();
    private readonly TollPassageService _service;

    public TollPassageServiceTests()
    {
        _service = new TollPassageService(_vehicleRepository, _tollPassageRepository);
    }

    [Fact]
    public async Task RecordTollPassageAsync_KnownVehicle_ReturnsThePersistedPassageId()
    {
        var expectedId = Guid.CreateVersion7();
        KnownVehicle("ABC123");
        _tollPassageRepository.AddAsync(Arg.Any<TollPassage>(), Arg.Any<CancellationToken>()).Returns(expectedId);

        var result = await _service.RecordTollPassageAsync("ABC123", PassedAt, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedId, result.Value);
        Assert.False(result.IsNotFound);
        Assert.False(result.IsInvalid);
    }

    [Fact]
    public async Task RecordTollPassageAsync_KnownVehicle_PersistsTheNormalizedRegistrationNumberAndTimestamp()
    {
        KnownVehicle(" abc123 ");

        await _service.RecordTollPassageAsync(" abc123 ", PassedAt, CancellationToken.None);

        await _tollPassageRepository.Received(1).AddAsync(
            Arg.Is<TollPassage>(passage => passage.RegistrationNumber == "ABC123" && passage.PassedAt == PassedAt),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RecordTollPassageAsync_UnknownVehicle_ReturnsNotFoundWithoutPersisting()
    {
        _vehicleRepository.GetAsync("XYZ789", Arg.Any<CancellationToken>()).Returns((Vehicle?)null);

        var result = await _service.RecordTollPassageAsync("XYZ789", PassedAt, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsNotFound);
        Assert.Contains("XYZ789", result.ErrorMessage);

        await _tollPassageRepository.DidNotReceiveWithAnyArgs().AddAsync(null!, CancellationToken.None);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RecordTollPassageAsync_BlankRegistrationNumber_ReturnsInvalidWithoutLookup(string? registrationNumber)
    {
        var result = await _service.RecordTollPassageAsync(registrationNumber!, PassedAt, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsInvalid);

        await _vehicleRepository.DidNotReceiveWithAnyArgs().GetAsync(null!, CancellationToken.None);
        await _tollPassageRepository.DidNotReceiveWithAnyArgs().AddAsync(null!, CancellationToken.None);
    }

    private void KnownVehicle(string lookupValue)
    {
        var vehicle = Vehicle.Create(lookupValue, VehicleType.Car);

        _vehicleRepository.GetAsync(lookupValue, Arg.Any<CancellationToken>()).Returns(vehicle);
    }
}
