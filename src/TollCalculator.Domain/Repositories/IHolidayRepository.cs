using TollCalculator.Domain.Models;

namespace TollCalculator.Domain.Repositories;

public interface IHolidayRepository
{
    Task<Holiday[]> GetAllAsync(CancellationToken cancellationToken);

    Task<Holiday[]> GetByYearAsync(int year, CancellationToken cancellationToken);
}
