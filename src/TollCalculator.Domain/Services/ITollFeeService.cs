using TollCalculator.Domain.Models;

namespace TollCalculator.Domain.Services;

public interface ITollFeeService
{
    int CalculateFee(Vehicle vehicle, DateTimeOffset passedAt, TollSchedule schedule, Holiday[] holidays);
}
