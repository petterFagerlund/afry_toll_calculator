using TollCalculator.Domain.Constants;
using TollCalculator.Domain.Enums;
using TollCalculator.Domain.Models;

namespace TollCalculator.Domain.Services.Implementations;

public sealed class TollFeeService : ITollFeeService
{
    private readonly ITollFreeDateService _tollFreeDateService;

    public TollFeeService(ITollFreeDateService tollFreeDateService)
    {
        ArgumentNullException.ThrowIfNull(tollFreeDateService);

        _tollFreeDateService = tollFreeDateService;
    }

    public int CalculateFee(VehicleType vehicleType, DateTimeOffset passedAt, TollSchedule schedule)
    {
        ArgumentNullException.ThrowIfNull(schedule);

        if (TollFreeVehicleTypes.Contains(vehicleType))
        {
            return 0;
        }

        var localPassedAt = TollTimeZone.ToLocalTime(passedAt);

        if (_tollFreeDateService.IsTollFree(DateOnly.FromDateTime(localPassedAt)))
        {
            return 0;
        }

        return schedule.FeeFor(TimeOnly.FromDateTime(localPassedAt));
    }
}
