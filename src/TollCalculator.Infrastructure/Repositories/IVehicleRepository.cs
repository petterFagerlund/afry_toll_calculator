using TollCalculator.Domain.Models;

namespace TollCalculator.Infrastructure.Repositories;

public interface IVehicleRepository
{
    Task<Vehicle?> GetAsync(string registrationNumber, CancellationToken cancellationToken);
}
