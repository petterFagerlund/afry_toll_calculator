using TollCalculator.Domain.Enums;
using TollCalculator.Domain.Models;
using TollCalculator.Domain.Repositories;
using TollCalculator.Infrastructure.Repositories.Implementations;
using Xunit;

namespace TollCalculator.Infrastructure.UnitTests.Repositories;

public class TollPassageRepositoryTests : IDisposable
{
    private static readonly TimeSpan Offset = TimeSpan.FromHours(2);
    private readonly RepositoryTestContext _fixture = new();
    private readonly ITollPassageRepository _repository;

    public TollPassageRepositoryTests()
    {
        _repository = new TollPassageRepository(_fixture.Context);

        _fixture.Context.Vehicles.Add(Vehicle.Create("ABC123", VehicleType.Car));
        _fixture.Context.Vehicles.Add(Vehicle.Create("XYZ789", VehicleType.Car));
        _fixture.Context.SaveChanges();
    }

    [Fact]
    public async Task AddAsync_NewPassage_IsPersisted()
    {
        var passage = TollPassage.Create("ABC123", new DateTimeOffset(2026, 10, 23, 7, 15, 0, Offset));

        var id = await _repository.AddAsync(passage, CancellationToken.None);

        Assert.Equal(passage.Id, id);
        Assert.Single(_fixture.Context.TollPassages);
    }

    [Fact]
    public async Task GetForDayAsync_PassagesOnThatDay_ReturnsThem()
    {
        await AddPassage("ABC123", new DateTimeOffset(2026, 10, 23, 7, 15, 0, Offset));
        await AddPassage("ABC123", new DateTimeOffset(2026, 10, 23, 16, 40, 0, Offset));
        await AddPassage("ABC123", new DateTimeOffset(2026, 10, 24, 7, 15, 0, Offset));

        var passages = await _repository.GetForDayAsync(
            "ABC123", new DateOnly(2026, 10, 23), CancellationToken.None);

        Assert.Equal(2, passages.Length);
    }

    [Fact]
    public async Task GetForDayAsync_OtherVehicle_IsExcluded()
    {
        await AddPassage("ABC123", new DateTimeOffset(2026, 10, 23, 7, 15, 0, Offset));
        await AddPassage("XYZ789", new DateTimeOffset(2026, 10, 23, 7, 15, 0, Offset));

        var passages = await _repository.GetForDayAsync(
            "ABC123", new DateOnly(2026, 10, 23), CancellationToken.None);

        Assert.Equal("ABC123", Assert.Single(passages).RegistrationNumber);
    }

    [Fact]
    public async Task GetForDayAsync_JustBeforeLocalMidnight_BelongsToThatDay()
    {
        await AddPassage("ABC123", new DateTimeOffset(2026, 10, 23, 23, 45, 0, Offset));

        var passages = await _repository.GetForDayAsync(
            "ABC123", new DateOnly(2026, 10, 23), CancellationToken.None);

        Assert.Single(passages);
    }

    [Fact]
    public async Task GetForDayAsync_NoPassages_ReturnsEmpty()
    {
        var passages = await _repository.GetForDayAsync(
            "ABC123", new DateOnly(2026, 10, 23), CancellationToken.None);

        Assert.Empty(passages);
    }

    [Fact]
    public async Task GetForDayAsync_ReturnsPassagesInChronologicalOrder()
    {
        await AddPassage("ABC123", new DateTimeOffset(2026, 10, 23, 16, 40, 0, Offset));
        await AddPassage("ABC123", new DateTimeOffset(2026, 10, 23, 7, 15, 0, Offset));

        var passages = await _repository.GetForDayAsync(
            "ABC123", new DateOnly(2026, 10, 23), CancellationToken.None);

        Assert.True(passages[0].PassedAt < passages[1].PassedAt);
    }

    public void Dispose()
    {
        _fixture.Dispose();
    }

    private async Task AddPassage(string registrationNumber, DateTimeOffset passedAt)
    {
        var passage = TollPassage.Create(registrationNumber, passedAt);

        await _repository.AddAsync(passage, CancellationToken.None);
    }
}
