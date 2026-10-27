using TollCalculator.Domain.Models;

namespace TollCalculator.Application.Services;

public interface ITollRateService
{
    Task<TollRateInterval[]> GetTollRatesAsync(CancellationToken cancellationToken);
}
