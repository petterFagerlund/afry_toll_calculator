using TollCalculator.Domain.Models;
using TollCalculator.Domain.Repositories;

namespace TollCalculator.Application.Services.Implementations;

internal sealed class VehicleTypeService : IVehicleTypeService
{
    private readonly IVehicleTypeRuleRepository _vehicleTypeRuleRepository;

    public VehicleTypeService(IVehicleTypeRuleRepository vehicleTypeRuleRepository)
    {
        ArgumentNullException.ThrowIfNull(vehicleTypeRuleRepository);

        _vehicleTypeRuleRepository = vehicleTypeRuleRepository;
    }

    public async Task<VehicleTypeRule[]> GetVehicleTypeRulesAsync(CancellationToken cancellationToken)
    {
        return await _vehicleTypeRuleRepository.GetAllAsync(cancellationToken);
    }
}
