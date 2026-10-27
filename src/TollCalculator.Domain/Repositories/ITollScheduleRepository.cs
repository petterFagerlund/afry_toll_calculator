using TollCalculator.Domain.Models;

namespace TollCalculator.Domain.Repositories;

public interface ITollScheduleRepository
{
    Task<TollTariff> GetTariffAsync(CancellationToken cancellationToken);

    Task<TollRateInterval[]> GetRateIntervalsAsync(CancellationToken cancellationToken);
}
