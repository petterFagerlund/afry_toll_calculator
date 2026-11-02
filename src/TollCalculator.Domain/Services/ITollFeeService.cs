using TollCalculator.Domain.Enums;
using TollCalculator.Domain.Models;

namespace TollCalculator.Domain.Services;

public interface ITollFeeService
{
    int CalculateFee(VehicleType vehicleType, DateTimeOffset passedAt, TollSchedule schedule);
}
