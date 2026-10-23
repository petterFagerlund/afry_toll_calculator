using TollCalculator.Domain.Models;

namespace TollCalculator.Domain.Services;

public interface IDailyTollService
{
    DailyToll Calculate(Vehicle vehicle, TollPassage[] passages, TollSchedule schedule, int dailyCap);
}
