using TollCalculator.Domain.Models;
using TollCalculator.WebApi.Models.V1.Responses;

namespace TollCalculator.WebApi.Host.Mappers;

public interface IDailyTollMapper
{
    DailyTollResponse ToDailyTollResponse(string registrationNumber, DateOnly date, DailyToll dailyToll);

    CalculateTollResponse ToCalculateTollResponse(string vehicleType, DailyToll dailyToll);
}
