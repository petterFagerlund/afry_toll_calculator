using TollCalculator.Domain.Models;
using TollCalculator.WebApi.Models.V1.Responses;
using ChargedTollPassageResponse = TollCalculator.WebApi.Models.V1.Responses.ChargedTollPassage;
using DomainChargedTollPassage = TollCalculator.Domain.Models.ChargedTollPassage;

namespace TollCalculator.WebApi.Host.Mappers.Implementations;

internal sealed class DailyTollMapper : IDailyTollMapper
{
    public DailyTollResponse ToDailyTollResponse(string registrationNumber, DateOnly date, DailyToll dailyToll)
    {
        ArgumentNullException.ThrowIfNull(dailyToll);

        return new DailyTollResponse
        {
            RegistrationNumber = registrationNumber,
            Date = date,
            TotalFee = dailyToll.TotalFee,
            ChargedPassages = ToChargedPassages(dailyToll.ChargedPassages),
        };
    }

    public CalculateTollResponse ToCalculateTollResponse(string vehicleType, DailyToll dailyToll)
    {
        ArgumentNullException.ThrowIfNull(dailyToll);

        return new CalculateTollResponse
        {
            VehicleType = vehicleType,
            TotalFee = dailyToll.TotalFee,
            ChargedPassages = ToChargedPassages(dailyToll.ChargedPassages),
        };
    }

    private static ChargedTollPassageResponse[] ToChargedPassages(DomainChargedTollPassage[] chargedPassages)
    {
        var responses = new ChargedTollPassageResponse[chargedPassages.Length];

        for (var index = 0; index < chargedPassages.Length; index++)
        {
            responses[index] = new ChargedTollPassageResponse
            {
                PassedAt = chargedPassages[index].PassedAt,
                Amount = chargedPassages[index].Amount,
            };
        }

        return responses;
    }
}
