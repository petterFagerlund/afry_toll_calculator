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

        var responses = new List<TollRateResponse>();

        foreach (var interval in intervals)
        {
            responses.Add(new TollRateResponse
            {
                Start = Format(interval.Start),
                End = Format(interval.End),
                Amount = interval.Amount,
            });
        }

        return responses.ToArray();
    }

    private static string Format(TimeOnly timeOfDay)
    {
        return timeOfDay.ToString(TimeOfDayFormat, CultureInfo.InvariantCulture);
    }
}
