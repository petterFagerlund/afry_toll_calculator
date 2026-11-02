using TollCalculator.Application.Results;
using TollCalculator.Domain.Enums;
using TollCalculator.Domain.Models;

namespace TollCalculator.Application.Services;

public interface IDailyTollService
{
    ApplicationResult<DailyToll> Calculate(VehicleType vehicleType, DateTimeOffset[] timeStamps, TollTariff tariff);
}
