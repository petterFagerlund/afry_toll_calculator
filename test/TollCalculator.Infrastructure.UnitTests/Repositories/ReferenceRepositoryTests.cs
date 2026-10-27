using TollCalculator.Domain.Enums;
using TollCalculator.Domain.Models;
using TollCalculator.Infrastructure.Repositories.Implementations;
using Xunit;

namespace TollCalculator.Infrastructure.UnitTests.Repositories;

public class ReferenceRepositoryTests : IDisposable
{
    private readonly RepositoryTestContext _fixture = new();

    [Fact]
    public async Task GetTariffAsync_ReturnsTheSeededSchedule()
    {
        var repository = new TollScheduleRepository(_fixture.Context);

        var tariff = await repository.GetTariffAsync(CancellationToken.None);

        Assert.Equal(9, tariff.Schedule.Intervals.Count);
        Assert.Equal(18, tariff.Schedule.FeeFor(new TimeOnly(7, 30)));
        Assert.Equal(8, tariff.Schedule.FeeFor(new TimeOnly(9, 5)));
    }

    [Fact]
    public async Task GetTariffAsync_ReturnsTheSeededCap()
    {
        var repository = new TollScheduleRepository(_fixture.Context);

        var tariff = await repository.GetTariffAsync(CancellationToken.None);

        Assert.Equal(60, tariff.DailyCap);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEverySeededHoliday()
    {
        var repository = new HolidayRepository(_fixture.Context);

        var holidays = await repository.GetAllAsync(CancellationToken.None);

        Assert.Equal(26, holidays.Length);
    }

    [Fact]
    public async Task GetByYearAsync_ReturnsOnlyThatYear()
    {
        var repository = new HolidayRepository(_fixture.Context);

        var holidays = await repository.GetByYearAsync(2026, CancellationToken.None);

        Assert.Equal(13, holidays.Length);
        Assert.All(holidays, holiday => Assert.Equal(2026, holiday.Date.Year));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEveryVehicleTypeRule()
    {
        var repository = new VehicleTypeRuleRepository(_fixture.Context);

        var rules = await repository.GetAllAsync(CancellationToken.None);

        Assert.Equal(7, rules.Length);
        Assert.Equal(6, rules.Count(rule => rule.IsTollFree));
    }

    [Fact]
    public async Task GetAsync_KnownVehicle_IsReturned()
    {
        _fixture.Context.Vehicles.Add(Vehicle.Create("ABC123", VehicleType.Car));
        await _fixture.Context.SaveChangesAsync(CancellationToken.None);
        var repository = new VehicleRepository(_fixture.Context);

        var vehicle = await repository.GetAsync("abc123", CancellationToken.None);

        Assert.NotNull(vehicle);
        Assert.Equal(VehicleType.Car, vehicle.VehicleType);
    }

    [Fact]
    public async Task GetAsync_UnknownVehicle_ReturnsNull()
    {
        var repository = new VehicleRepository(_fixture.Context);

        var vehicle = await repository.GetAsync("NOPE123", CancellationToken.None);

        Assert.Null(vehicle);
    }

    public void Dispose()
    {
        _fixture.Dispose();
    }
}
