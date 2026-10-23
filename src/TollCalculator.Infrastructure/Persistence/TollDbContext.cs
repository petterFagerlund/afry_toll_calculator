using Microsoft.EntityFrameworkCore;
using TollCalculator.Domain.Models;

namespace TollCalculator.Infrastructure.Persistence;

public sealed class TollDbContext : DbContext
{
    public TollDbContext(DbContextOptions<TollDbContext> options) : base(options)
    {
    }

    public DbSet<Vehicle> Vehicles { get; init; } = null!;

    public DbSet<TollPassage> TollPassages { get; init; } = null!;

    public DbSet<TollRateInterval> TollRateIntervals { get; init; } = null!;

    public DbSet<Holiday> Holidays { get; init; } = null!;

    public DbSet<TollSettings> TollSettings { get; init; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TollDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
