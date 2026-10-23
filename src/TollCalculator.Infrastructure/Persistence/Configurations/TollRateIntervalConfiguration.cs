using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TollCalculator.Domain.Models;
using TollCalculator.Infrastructure.Persistence.Seed;

namespace TollCalculator.Infrastructure.Persistence.Configurations;

internal sealed class TollRateIntervalConfiguration : IEntityTypeConfiguration<TollRateInterval>
{
    public void Configure(EntityTypeBuilder<TollRateInterval> builder)
    {
        builder.ToTable("TollRateIntervals");

        builder.Property<int>("Id")
            .ValueGeneratedOnAdd();

        builder.HasKey("Id");

        builder.Property(interval => interval.Start);
        builder.Property(interval => interval.End);
        builder.Property(interval => interval.Amount);

        builder.HasIndex(interval => interval.Start)
            .IsUnique();

        foreach (var seed in TollSeedData.RateIntervals)
        {
            builder.HasData(new { seed.Id, seed.Start, seed.End, seed.Amount });
        }
    }
}
