using TollCalculator.Application.Results;
using TollCalculator.Domain.Models;

namespace TollCalculator.Application.Services;

public interface IHolidayService
{
    Task<ApplicationResult<Holiday[]>> GetHolidaysAsync(int? year, CancellationToken cancellationToken);
}
