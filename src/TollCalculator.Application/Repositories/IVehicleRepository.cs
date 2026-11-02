using TollCalculator.Domain.Models;

namespace TollCalculator.Application.Repositories;

public interface IVehicleRepository
{
    Task<Vehicle?> GetAsync(string registrationNumber, CancellationToken cancellationToken);
}
