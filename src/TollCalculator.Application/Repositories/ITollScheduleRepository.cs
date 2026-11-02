using TollCalculator.Domain.Models;

namespace TollCalculator.Application.Repositories;

public interface ITollScheduleRepository
{
    Task<TollTariff> GetTariffAsync(CancellationToken cancellationToken);

    Task<TollRateInterval[]> GetRateIntervalsAsync(CancellationToken cancellationToken);
}
