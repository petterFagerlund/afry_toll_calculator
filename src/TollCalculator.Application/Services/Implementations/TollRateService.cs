using TollCalculator.Application.Results;
using TollCalculator.Domain.Models;
using TollCalculator.Infrastructure.Repositories;

namespace TollCalculator.Application.Services.Implementations;

internal sealed class TollRateService : ITollRateService
{
    private readonly ITollScheduleRepository _tollScheduleRepository;

    public TollRateService(ITollScheduleRepository tollScheduleRepository)
    {
        ArgumentNullException.ThrowIfNull(tollScheduleRepository);

        _tollScheduleRepository = tollScheduleRepository;
    }

    public async Task<ApplicationResult<TollRateInterval[]>> GetTollRatesAsync(CancellationToken cancellationToken)
    {
        var schedule = await _tollScheduleRepository.GetScheduleAsync(cancellationToken);

        return ApplicationResult<TollRateInterval[]>.Success(schedule.Intervals.ToArray());
    }
}
