using TollCalculator.Application.Results;
using TollCalculator.Domain.Models;

namespace TollCalculator.Application.Services;

public interface IDailyTollService
{
    ApplicationResult<DailyToll> Calculate(TollPassage[] passages, TollTariff tariff);
}
