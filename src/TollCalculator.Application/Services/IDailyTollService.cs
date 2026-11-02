using TollCalculator.Application.Results;
using TollCalculator.Domain.Models;

namespace TollCalculator.Application.Services;

public interface IDailyTollService
{
    /// <summary>Charges one Stockholm day of passages, applying the hourly window and the daily cap.</summary>
    /// <param name="passages">
    /// The passages for a single Stockholm day. The daily cap is applied once across all of them, so passing
    /// passages from more than one day would cap them together rather than per day.
    /// </param>
    Task<ApplicationResult<DailyToll>> Calculate(TollPassage[] passages, CancellationToken cancellationToken);
}