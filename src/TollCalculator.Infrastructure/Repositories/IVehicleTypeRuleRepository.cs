using TollCalculator.Domain.Models;

namespace TollCalculator.Infrastructure.Repositories;

public interface IVehicleTypeRuleRepository
{
    Task<VehicleTypeRule[]> GetAllAsync(CancellationToken cancellationToken);
}
