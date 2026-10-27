using TollCalculator.Domain.Models;
using TollCalculator.Domain.Repositories;

namespace TollCalculator.Application.Services.Implementations;

internal sealed class TollRateService : ITollRateService
{
    private readonly ITollScheduleRepository _tollScheduleRepository;

    public TollRateService(ITollScheduleRepository tollScheduleRepository)
    {
        ArgumentNullException.ThrowIfNull(tollScheduleRepository);

        _tollScheduleRepository = tollScheduleRepository;
    }

    public async Task<TollRateInterval[]> GetTollRatesAsync(CancellationToken cancellationToken)
    {
        return await _tollScheduleRepository.GetRateIntervalsAsync(cancellationToken);
    }
}
