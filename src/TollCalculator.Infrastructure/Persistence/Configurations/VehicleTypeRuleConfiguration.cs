using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TollCalculator.Domain.Models;
using TollCalculator.Infrastructure.Persistence.Seed;

namespace TollCalculator.Infrastructure.Persistence.Configurations;

internal sealed class VehicleTypeRuleConfiguration : IEntityTypeConfiguration<VehicleTypeRule>
{
    public void Configure(EntityTypeBuilder<VehicleTypeRule> builder)
    {
        builder.ToTable("VehicleTypeRules");

        builder.HasKey(rule => rule.VehicleType);

        builder.Property(rule => rule.VehicleType)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(rule => rule.IsTollFree);

        builder.HasData(TollSeedData.VehicleTypeRules);
    }
}
