using TollCalculator.Application.Results;

namespace TollCalculator.Application.Services;

public interface ITollFreeDateService
{
    ApplicationResult<bool> IsTollFree(DateOnly date);
}
