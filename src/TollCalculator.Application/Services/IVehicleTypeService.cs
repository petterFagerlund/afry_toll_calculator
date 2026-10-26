using TollCalculator.Application.Results;
using TollCalculator.Domain.Models;

namespace TollCalculator.Application.Services;

public interface IVehicleTypeService
{
    Task<ApplicationResult<VehicleTypeRule[]>> GetVehicleTypeRulesAsync(CancellationToken cancellationToken);
}
