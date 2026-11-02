using TollCalculator.Application.Results;
using TollCalculator.Domain.Enums;
using TollCalculator.Domain.Models;

namespace TollCalculator.Application.Services;

public interface ITollFeeService
{
    ApplicationResult<int> CalculateFee(VehicleType vehicleType, DateTimeOffset passedAt, TollSchedule schedule);
}
