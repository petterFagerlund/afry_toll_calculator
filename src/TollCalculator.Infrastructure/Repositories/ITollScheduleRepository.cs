using TollCalculator.Domain.Models;

namespace TollCalculator.Infrastructure.Repositories;

public interface ITollScheduleRepository
{
    Task<TollSchedule> GetScheduleAsync(CancellationToken cancellationToken);

    Task<int> GetDailyCapAsync(CancellationToken cancellationToken);
}
