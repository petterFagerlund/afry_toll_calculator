using TollCalculator.Application.Results;
using TollCalculator.Domain.Models;

namespace TollCalculator.Application.Services;

public interface ITollService
{
    Task<ApplicationResult<DailyToll>> GetDailyTollAsync(
        string registrationNumber,
        DateOnly date,
        CancellationToken cancellationToken);

    Task<ApplicationResult<DailyToll>> CalculateAsync(
        string vehicleType,
        DateTimeOffset[] timestamps,
        CancellationToken cancellationToken);
}
