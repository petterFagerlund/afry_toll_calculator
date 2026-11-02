using Microsoft.Extensions.Logging;
using TollCalculator.Application.Results;
using TollCalculator.Domain.Models;
using TollCalculator.Application.Repositories;

namespace TollCalculator.Application.Services.Implementations;

internal sealed class TollPassageService : ITollPassageService
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly ITollPassageRepository _tollPassageRepository;
    private readonly ILogger<TollPassageService> _logger;

    public TollPassageService(
        IVehicleRepository vehicleRepository,
        ITollPassageRepository tollPassageRepository,
        ILogger<TollPassageService> logger)
    {
        ArgumentNullException.ThrowIfNull(vehicleRepository);
        ArgumentNullException.ThrowIfNull(tollPassageRepository);
        ArgumentNullException.ThrowIfNull(logger);

        _vehicleRepository = vehicleRepository;
        _tollPassageRepository = tollPassageRepository;
        _logger = logger;
    }

    public async Task<ApplicationResult<Guid>> RecordTollPassageAsync(
        string registrationNumber,
        DateTimeOffset passedAt,
        CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleRepository.GetAsync(registrationNumber, cancellationToken);

        if (vehicle is null)
        {
            _logger.LogInformation(
                "No vehicle is registered with {RegistrationNumber}; the passage was not recorded.",
                registrationNumber);

            return ApplicationResult<Guid>.NotFound(
                $"No vehicle is registered with the number '{RegistrationNumbers.Normalize(registrationNumber)}'.");
        }

        var passage = TollPassage.Create(vehicle.RegistrationNumber, passedAt);

        var added = await _tollPassageRepository.AddAsync(passage, cancellationToken);

        if (!added.IsSuccess)
        {
            _logger.LogInformation(
                "The passage for {RegistrationNumber} at {PassedAt} was not recorded: {Reason}",
                vehicle.RegistrationNumber,
                passedAt,
                added.ErrorMessage);

            return added;
        }

        _logger.LogInformation(
            "Recorded passage {TollPassageId} for {RegistrationNumber} at {PassedAt}.",
            added.Value,
            vehicle.RegistrationNumber,
            passedAt);

        return added;
    }
}
