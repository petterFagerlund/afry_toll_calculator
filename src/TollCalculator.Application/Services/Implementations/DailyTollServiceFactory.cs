using TollCalculator.Domain.Models;
using TollCalculator.Domain.Services;
using TollCalculator.Domain.Services.Implementations;

namespace TollCalculator.Application.Services.Implementations;

internal sealed class DailyTollServiceFactory : IDailyTollServiceFactory
{
    public IDailyTollService Create(Holiday[] holidays)
    {
        ArgumentNullException.ThrowIfNull(holidays);

        var tollFreeDateService = new WeekendAndHolidayService(holidays);
        var tollFeeService = new TollFeeService(tollFreeDateService);

        return new DailyTollService(tollFeeService);
    }
}
