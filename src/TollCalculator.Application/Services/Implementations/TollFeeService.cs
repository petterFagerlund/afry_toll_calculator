using TollCalculator.Application.Results;
using TollCalculator.Domain.Constants;
using TollCalculator.Domain.Enums;
using TollCalculator.Domain.Models;

namespace TollCalculator.Application.Services.Implementations;

public sealed class TollFeeService : ITollFeeService
{
    private readonly ITollFreeDateService _tollFreeDateService;

    public TollFeeService(ITollFreeDateService tollFreeDateService)
    {
        ArgumentNullException.ThrowIfNull(tollFreeDateService);

        _tollFreeDateService = tollFreeDateService;
    }

    public ApplicationResult<int> CalculateFee(VehicleType vehicleType, DateTimeOffset passedAt, TollSchedule schedule)
    {
        ArgumentNullException.ThrowIfNull(schedule);

        if (TollFreeVehicleTypes.Contains(vehicleType))
        {
            return ApplicationResult<int>.Success(0);
        }

        var localPassedAt = TollTimeZone.ToLocalTime(passedAt);
        var tollFreeDate = _tollFreeDateService.IsTollFree(DateOnly.FromDateTime(localPassedAt));

        if (!tollFreeDate.IsSuccess)
        {
            return ApplicationResult<int>.Error(tollFreeDate.ErrorMessage!);
        }

        if (tollFreeDate.Value)
        {
            return ApplicationResult<int>.Success(0);
        }

        return ApplicationResult<int>.Success(schedule.FeeFor(TimeOnly.FromDateTime(localPassedAt)));
    }
}
