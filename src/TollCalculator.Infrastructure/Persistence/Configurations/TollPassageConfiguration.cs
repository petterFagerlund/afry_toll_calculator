using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TollCalculator.Domain.Models;

namespace TollCalculator.Infrastructure.Persistence.Configurations;

internal sealed class TollPassageConfiguration : IEntityTypeConfiguration<TollPassage>
{
    public void Configure(EntityTypeBuilder<TollPassage> builder)
    {
        builder.ToTable("TollPassages");

        builder.HasKey(passage => passage.Id);

        builder.Property(passage => passage.RegistrationNumber)
            .HasMaxLength(20);

        builder.Property(passage => passage.VehicleType)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(passage => new { passage.RegistrationNumber, passage.PassedAt })
            .IsUnique();
    }
}
