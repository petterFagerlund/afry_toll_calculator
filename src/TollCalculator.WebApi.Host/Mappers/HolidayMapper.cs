using TollCalculator.Domain.Models;
using TollCalculator.WebApi.Models.V1.Responses;

namespace TollCalculator.WebApi.Host.Mappers;

internal static class HolidayMapper
{
    public static HolidayResponse[] ToResponses(Holiday[] holidays)
    {
        ArgumentNullException.ThrowIfNull(holidays);

        var responses = new List<HolidayResponse>();

        foreach (var holiday in holidays)
        {
            responses.Add(new HolidayResponse
            {
                Date = holiday.Date,
                Name = holiday.Name,
            });
        }

        return responses.ToArray();
    }
}
