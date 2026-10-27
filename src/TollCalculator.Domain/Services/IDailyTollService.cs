using TollCalculator.Domain.Models;

namespace TollCalculator.Domain.Services;

public interface IDailyTollService
{
    DailyToll Calculate(Vehicle vehicle, TollPassage[] passages, TollTariff tariff, Holiday[] holidays);
}
