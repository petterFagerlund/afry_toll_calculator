namespace TollCalculator.Domain.Services;

public interface ITollFreeDateService
{
    bool IsTollFree(DateOnly date);
}
