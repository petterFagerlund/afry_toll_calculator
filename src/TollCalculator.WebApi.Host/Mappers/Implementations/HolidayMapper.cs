using TollCalculator.Domain.Models;
using TollCalculator.WebApi.Models.V1.Responses;

namespace TollCalculator.WebApi.Host.Mappers.Implementations;

internal sealed class HolidayMapper : IHolidayMapper
{
    public HolidayResponse[] ToResponses(Holiday[] holidays)
    {
        ArgumentNullException.ThrowIfNull(holidays);

        var responses = new HolidayResponse[holidays.Length];

        for (var index = 0; index < holidays.Length; index++)
        {
            responses[index] = new HolidayResponse
            {
                Date = holidays[index].Date,
                Name = holidays[index].Name,
            };
        }

        return responses;
    }
}
