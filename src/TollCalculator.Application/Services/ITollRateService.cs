using TollCalculator.Application.Results;
using TollCalculator.Domain.Models;

namespace TollCalculator.Application.Services;

public interface ITollRateService
{
    Task<ApplicationResult<TollRateInterval[]>> GetTollRatesAsync(CancellationToken cancellationToken);
}
