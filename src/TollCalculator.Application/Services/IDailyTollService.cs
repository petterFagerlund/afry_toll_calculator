using TollCalculator.Application.Results;
using TollCalculator.Domain.Models;

namespace TollCalculator.Application.Services;

public interface IDailyTollService
{
    Task<ApplicationResult<DailyToll>> Calculate(TollPassage[] passages, CancellationToken cancellationToken);
}