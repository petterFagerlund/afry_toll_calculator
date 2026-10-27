using TollCalculator.Domain.Models;

namespace TollCalculator.Domain.Repositories;

public interface IVehicleRepository
{
    Task<Vehicle?> GetAsync(string registrationNumber, CancellationToken cancellationToken);
}
