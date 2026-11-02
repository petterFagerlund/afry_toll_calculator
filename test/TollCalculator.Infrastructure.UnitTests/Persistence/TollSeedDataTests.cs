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

    private IDictionary<string, object?>[] SeedFor(Type entityType)
    {
        var model = _context.GetService<IDesignTimeModel>().Model;

        return model.FindEntityType(entityType)!.GetSeedData().ToArray();
    }
}
