using TollCalculator.Application.Results;
using TollCalculator.Domain.Constants;
using TollCalculator.Domain.Enums;
using TollCalculator.Domain.Models;

namespace TollCalculator.Application.Services.Implementations;

internal sealed class TollFeeService : ITollFeeService
{
    private readonly ITollFreeDateService _tollFreeDateService;

    public TollFeeService(ITollFreeDateService tollFreeDateService)
    {
        ArgumentNullException.ThrowIfNull(tollFreeDateService);

        _tollFreeDateService = tollFreeDateService;
    }

    public ApplicationResult<int> CalculateFee(
        VehicleType vehicleType,
        DateTimeOffset passedAt,
        TollSchedule tollSchedule)
    {
        ArgumentNullException.ThrowIfNull(tollSchedule);

        if (TollFreeVehicleTypes.Contains(vehicleType))
        {
            return ApplicationResult<int>.Success(0);
        }

        var localPassedAt = TollTimeZone.ToLocalTime(passedAt);

        return _tollFreeDateService.IsTollFree(DateOnly.FromDateTime(localPassedAt)).Map(isTollFree =>
        {
            if (isTollFree)
            {
                return 0;
            }

            return tollSchedule.FeeFor(TimeOnly.FromDateTime(localPassedAt));
        });
    }
}
