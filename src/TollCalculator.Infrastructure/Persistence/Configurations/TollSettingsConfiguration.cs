using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TollCalculator.Domain.Models;
using TollCalculator.Infrastructure.Persistence.Seed;

namespace TollCalculator.Infrastructure.Persistence.Configurations;

internal sealed class TollSettingsConfiguration : IEntityTypeConfiguration<TollSettings>
{
    public void Configure(EntityTypeBuilder<TollSettings> builder)
    {
        builder.ToTable("TollSettings");

        builder.Property<int>("Id");

        builder.HasKey("Id");

        builder.Property(settings => settings.DailyCap);

        builder.HasData(new { Id = TollSeedData.SettingsId, DailyCap = TollSeedData.DailyCap });
    }
}
