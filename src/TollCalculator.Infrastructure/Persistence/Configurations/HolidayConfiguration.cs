using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TollCalculator.Domain.Models;
using TollCalculator.Infrastructure.Persistence.Seed;

namespace TollCalculator.Infrastructure.Persistence.Configurations;

internal sealed class HolidayConfiguration : IEntityTypeConfiguration<Holiday>
{
    public void Configure(EntityTypeBuilder<Holiday> builder)
    {
        builder.ToTable("Holidays");

        builder.HasKey(holiday => holiday.Date);

        builder.Property(holiday => holiday.Name)
            .HasMaxLength(100);

        builder.HasData(TollSeedData.Holidays);
    }
}
