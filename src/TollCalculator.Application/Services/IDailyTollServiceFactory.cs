using TollCalculator.Domain.Models;
using TollCalculator.Domain.Services;

namespace TollCalculator.Application.Services;

public interface IDailyTollServiceFactory
{
    IDailyTollService Create(Holiday[] holidays);
}
