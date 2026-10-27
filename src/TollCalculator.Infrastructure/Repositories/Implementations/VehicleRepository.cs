using Microsoft.EntityFrameworkCore;
using TollCalculator.Domain.Models;
using TollCalculator.Domain.Repositories;
using TollCalculator.Infrastructure.Persistence;

namespace TollCalculator.Infrastructure.Repositories.Implementations;

internal sealed class VehicleRepository : IVehicleRepository
{
    private readonly TollDbContext _context;

    public VehicleRepository(TollDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _context = context;
    }

    public async Task<Vehicle?> GetAsync(string registrationNumber, CancellationToken cancellationToken)
    {
        var normalized = RegistrationNumbers.Normalize(registrationNumber, nameof(registrationNumber));

        return await _context.Vehicles
            .AsNoTracking()
            .SingleOrDefaultAsync(vehicle => vehicle.RegistrationNumber == normalized, cancellationToken);
    }
}
