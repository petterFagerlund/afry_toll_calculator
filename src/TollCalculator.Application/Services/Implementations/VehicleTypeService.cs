using TollCalculator.Application.Results;
using TollCalculator.Domain.Models;
using TollCalculator.Infrastructure.Repositories;

namespace TollCalculator.Application.Services.Implementations;

internal sealed class VehicleTypeService : IVehicleTypeService
{
    private readonly IVehicleTypeRuleRepository _vehicleTypeRuleRepository;

    public VehicleTypeService(IVehicleTypeRuleRepository vehicleTypeRuleRepository)
    {
        ArgumentNullException.ThrowIfNull(vehicleTypeRuleRepository);

        _vehicleTypeRuleRepository = vehicleTypeRuleRepository;
    }

    public async Task<ApplicationResult<VehicleTypeRule[]>> GetVehicleTypeRulesAsync(
        CancellationToken cancellationToken)
    {
        var rules = await _vehicleTypeRuleRepository.GetAllAsync(cancellationToken);

        return ApplicationResult<VehicleTypeRule[]>.Success(rules);
    }
}
