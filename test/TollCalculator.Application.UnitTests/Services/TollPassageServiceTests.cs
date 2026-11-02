using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using TollCalculator.Application.Repositories;
using TollCalculator.Application.Results;
using TollCalculator.Application.Services.Implementations;
using TollCalculator.Domain.Enums;
using TollCalculator.Domain.Models;
using Xunit;

namespace TollCalculator.Application.UnitTests.Services;

public class TollPassageServiceTests
{
    private static readonly DateTimeOffset PassedAt = new(2026, 3, 16, 7, 15, 0, TimeSpan.FromHours(1));

    private readonly ITollPassageRepository _tollPassageRepository = Substitute.For<ITollPassageRepository>();
    private readonly TollPassageService _service;

    public TollPassageServiceTests()
    {
        _service = new TollPassageService(_tollPassageRepository, NullLogger<TollPassageService>.Instance);
    }

    [Fact]
    public async Task RecordTollPassagesAsync_ABatch_ReturnsThePersistedPassageIds()
    {
        var passages = new[]
        {
            TollPassage.Create("ABC123", VehicleType.Car, PassedAt),
            TollPassage.Create("XYZ789", VehicleType.Motorbike, PassedAt),
        };
        var expectedIds = Array.ConvertAll(passages, passage => passage.Id);

        _tollPassageRepository.AddRangeAsync(Arg.Any<TollPassage[]>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationResult<Guid[]>.Success(expectedIds));

        var result = await _service.RecordTollPassagesAsync(passages, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedIds, result.Value);
    }

    [Fact]
    public async Task RecordTollPassagesAsync_AnUnregisteredVehicle_IsStillRecorded()
    {
        var passages = new[] { TollPassage.Create("NOPE01", VehicleType.Car, PassedAt) };

        _tollPassageRepository.AddRangeAsync(Arg.Any<TollPassage[]>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationResult<Guid[]>.Success([passages[0].Id]));

        var result = await _service.RecordTollPassagesAsync(passages, CancellationToken.None);

        Assert.True(result.IsSuccess);

        await _tollPassageRepository.Received(1).AddRangeAsync(
            Arg.Is<TollPassage[]>(recorded => recorded[0].RegistrationNumber == "NOPE01"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RecordTollPassagesAsync_APassageAlreadyRecorded_ReturnsConflict()
    {
        var passages = new[] { TollPassage.Create("ABC123", VehicleType.Car, PassedAt) };

        _tollPassageRepository.AddRangeAsync(Arg.Any<TollPassage[]>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationResult<Guid[]>.Conflict("One or more of the passages is already recorded."));

        var result = await _service.RecordTollPassagesAsync(passages, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationResultStatus.Conflict, result.Status);
    }

    [Fact]
    public async Task RecordTollPassagesAsync_NullPassages_Throws()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _service.RecordTollPassagesAsync(null!, CancellationToken.None));
    }
}
