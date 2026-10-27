using TollCalculator.Domain.Models;

namespace TollCalculator.Application.Services;

public interface IVehicleTypeService
{
    Task<VehicleTypeRule[]> GetVehicleTypeRulesAsync(CancellationToken cancellationToken);
}
