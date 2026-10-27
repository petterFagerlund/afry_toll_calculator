using TollCalculator.Domain.Constants;
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

    public int CalculateFee(Vehicle vehicle, DateTimeOffset passedAt, TollSchedule schedule, Holiday[] holidays)
    {
        ArgumentNullException.ThrowIfNull(vehicle);
        ArgumentNullException.ThrowIfNull(schedule);
        ArgumentNullException.ThrowIfNull(holidays);

        if (vehicle.IsTollFree)
        {
            return 0;
        }

        var localPassedAt = TollTimeZone.ToLocalTime(passedAt);

        if (_tollFreeDateService.IsTollFree(DateOnly.FromDateTime(localPassedAt), holidays))
        {
            return 0;
        }

        return schedule.FeeFor(TimeOnly.FromDateTime(localPassedAt));
    }
}
