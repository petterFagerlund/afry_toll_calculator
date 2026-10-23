using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using TollCalculator.Domain.Enums;
using TollCalculator.Domain.Models;
using TollCalculator.Infrastructure.Persistence;
using Xunit;

namespace TollCalculator.Infrastructure.UnitTests.Persistence;

public class TollSeedDataTests
{
    private readonly TollDbContext _context;

    public TollSeedDataTests()
    {
        var options = new DbContextOptionsBuilder<TollDbContext>()
            .UseSqlServer("Server=localhost;Database=TollCalculatorModelOnly;Trusted_Connection=True")
            .Options;

        _context = new TollDbContext(options);
    }

    [Fact]
    public void Seed_RateIntervals_CoverTheWholeChargedDay()
    {
        var seeded = SeedFor(typeof(TollRateInterval));

        Assert.Equal(9, seeded.Length);
        Assert.Equal(new TimeOnly(6, 0), seeded.Min(row => (TimeOnly)row["Start"]!));
        Assert.Equal(new TimeOnly(18, 29), seeded.Max(row => (TimeOnly)row["End"]!));
    }

    [Theory]
    [InlineData(7, 30, 18)]
    [InlineData(9, 5, 8)]
    [InlineData(15, 0, 13)]
    public void Seed_RateIntervals_ChargeTheScheduledAmount(int hour, int minute, int expected)
    {
        var timeOfDay = new TimeOnly(hour, minute);

        var match = SeedFor(typeof(TollRateInterval))
            .Single(row => (TimeOnly)row["Start"]! <= timeOfDay && timeOfDay <= (TimeOnly)row["End"]!);

        Assert.Equal(expected, match["Amount"]);
    }

    [Fact]
    public void Seed_TollSettings_HasTheDailyCap()
    {
        var settings = Assert.Single(SeedFor(typeof(TollSettings)));

        Assert.Equal(60, settings["DailyCap"]);
    }

    [Fact]
    public void Seed_VehicleTypeRules_CoverEveryTypeExceptUnknown()
    {
        var seeded = SeedFor(typeof(VehicleTypeRule));

        Assert.Equal(Enum.GetValues<VehicleType>().Length - 1, seeded.Length);
        Assert.DoesNotContain(seeded, row => row["VehicleType"]!.Equals(VehicleType.Unknown));
    }

    [Fact]
    public void Seed_VehicleTypeRules_MarkSixTypesTollFree()
    {
        var seeded = SeedFor(typeof(VehicleTypeRule));

        Assert.Equal(6, seeded.Count(row => (bool)row["IsTollFree"]!));
        Assert.Single(seeded, row => row["VehicleType"]!.Equals(VehicleType.Car) && !(bool)row["IsTollFree"]!);
    }

    [Fact]
    public void Seed_Holidays_CoverTwoYears()
    {
        var seeded = SeedFor(typeof(Holiday));
        var years = seeded.Select(row => ((DateOnly)row["Date"]!).Year).Distinct().ToArray();

        Assert.Equal(26, seeded.Length);
        Assert.Equal([2026, 2027], years.Order());
    }

    [Theory]
    [InlineData(2026, 4, 3)]
    [InlineData(2026, 6, 20)]
    [InlineData(2027, 3, 26)]
    [InlineData(2027, 6, 26)]
    public void Seed_Holidays_IncludeTheMovableOnes(int year, int month, int day)
    {
        var seeded = SeedFor(typeof(Holiday));

        Assert.Contains(seeded, row => (DateOnly)row["Date"]! == new DateOnly(year, month, day));
    }

    [Fact]
    public void Seed_Holidays_HaveNames()
    {
        var seeded = SeedFor(typeof(Holiday));

        Assert.All(seeded, row => Assert.False(string.IsNullOrWhiteSpace((string?)row["Name"])));
    }

    private IDictionary<string, object?>[] SeedFor(Type entityType)
    {
        var model = _context.GetService<IDesignTimeModel>().Model;

        return model.FindEntityType(entityType)!.GetSeedData().ToArray();
    }
}
