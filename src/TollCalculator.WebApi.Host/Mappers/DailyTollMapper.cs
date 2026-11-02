using TollCalculator.Domain.Models;
using TollCalculator.WebApi.Models.V1.Responses;
using ChargedTollPassageResponse = TollCalculator.WebApi.Models.V1.Responses.ChargedTollPassage;
using DomainChargedTollPassage = TollCalculator.Domain.Models.ChargedTollPassage;

namespace TollCalculator.WebApi.Host.Mappers;

internal static class DailyTollMapper
{
    public static DailyTollResponse ToDailyTollResponse(string registrationNumber, DateOnly date, DailyToll dailyToll)
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

    private static ChargedTollPassageResponse[] ToChargedPassages(DomainChargedTollPassage[] chargedPassages)
    {
        return Array.ConvertAll(chargedPassages, chargedPassage => new ChargedTollPassageResponse
        {
            PassedAt = chargedPassage.PassedAt,
            Amount = chargedPassage.Amount,
        });
    }
}
