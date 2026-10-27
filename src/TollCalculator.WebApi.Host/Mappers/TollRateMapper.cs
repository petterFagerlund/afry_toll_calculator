using System.Globalization;
using TollCalculator.Domain.Models;
using TollCalculator.WebApi.Models.V1.Responses;

namespace TollCalculator.WebApi.Host.Mappers;

internal static class TollRateMapper
{
    private const string TimeOfDayFormat = @"HH\:mm";

    public static TollRateResponse[] ToResponses(TollRateInterval[] intervals)
    {
        ArgumentNullException.ThrowIfNull(intervals);

        var responses = new TollRateResponse[intervals.Length];

        for (var index = 0; index < intervals.Length; index++)
        {
            responses[index] = new TollRateResponse
            {
                Start = Format(intervals[index].Start),
                End = Format(intervals[index].End),
                Amount = intervals[index].Amount,
            };
        }

        return responses;
    }

    private static string Format(TimeOnly timeOfDay)
    {
        return timeOfDay.ToString(TimeOfDayFormat, CultureInfo.InvariantCulture);
    }
}
