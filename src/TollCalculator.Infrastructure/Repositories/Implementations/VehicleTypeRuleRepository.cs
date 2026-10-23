using Microsoft.EntityFrameworkCore;
using TollCalculator.Domain.Models;
using TollCalculator.Infrastructure.Persistence;

namespace TollCalculator.Infrastructure.Repositories.Implementations;

internal sealed class VehicleTypeRuleRepository : IVehicleTypeRuleRepository
{
    private readonly TollDbContext _context;

    public VehicleTypeRuleRepository(TollDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _context = context;
    }

    public async Task<VehicleTypeRule[]> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.VehicleTypeRules
            .AsNoTracking()
            .OrderBy(rule => rule.VehicleType)
            .ToArrayAsync(cancellationToken);
    }
}
