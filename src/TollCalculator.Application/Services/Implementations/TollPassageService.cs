using TollCalculator.Application.Results;
using TollCalculator.Domain.Models;
using TollCalculator.Domain.Repositories;

namespace TollCalculator.Application.Services.Implementations;

internal sealed class TollPassageService : ITollPassageService
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly ITollPassageRepository _tollPassageRepository;

    public TollPassageService(IVehicleRepository vehicleRepository, ITollPassageRepository tollPassageRepository)
    {
        ArgumentNullException.ThrowIfNull(vehicleRepository);
        ArgumentNullException.ThrowIfNull(tollPassageRepository);

        _vehicleRepository = vehicleRepository;
        _tollPassageRepository = tollPassageRepository;
    }

    public async Task<ApplicationResult<Guid>> RecordTollPassageAsync(
        string registrationNumber,
        DateTimeOffset passedAt,
        CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleRepository.GetAsync(registrationNumber, cancellationToken);

        if (vehicle is null)
        {
            return ApplicationResult<Guid>.NotFound(
                $"No vehicle is registered with the number '{registrationNumber.Trim().ToUpperInvariant()}'.");
        }

        var passage = TollPassage.Create(vehicle.RegistrationNumber, passedAt);

        var id = await _tollPassageRepository.AddAsync(passage, cancellationToken);

        return ApplicationResult<Guid>.Success(id);
    }
}
