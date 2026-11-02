using TollCalculator.Domain.Enums;
using TollCalculator.Domain.Models;

namespace TollCalculator.Domain.Services;

public interface IDailyTollService
{
    DailyToll Calculate(VehicleType vehicleType, DateTimeOffset[] passedAt, TollTariff tariff);
}
