using TollCalculator.Domain.Models;

namespace TollCalculator.Domain.Repositories;

public interface IVehicleTypeRuleRepository
{
    Task<VehicleTypeRule[]> GetAllAsync(CancellationToken cancellationToken);
}
