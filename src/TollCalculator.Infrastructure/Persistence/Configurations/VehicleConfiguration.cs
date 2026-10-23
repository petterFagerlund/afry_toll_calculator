using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TollCalculator.Domain.Enums;
using TollCalculator.Domain.Models;

namespace TollCalculator.Infrastructure.Persistence.Configurations;

internal sealed class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("Vehicles");

        builder.HasKey(vehicle => vehicle.RegistrationNumber);

        builder.Property(vehicle => vehicle.RegistrationNumber)
            .HasMaxLength(20);

        builder.Property(vehicle => vehicle.VehicleType)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Ignore(vehicle => vehicle.IsTollFree);
    }
}
