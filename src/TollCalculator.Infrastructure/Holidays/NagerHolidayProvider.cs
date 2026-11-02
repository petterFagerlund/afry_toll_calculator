using System.Collections.Concurrent;
using Nager.Date;
using TollCalculator.Domain.Models;
using TollCalculator.Domain.Services;

namespace TollCalculator.Infrastructure.Holidays;

internal sealed class NagerHolidayProvider : IHolidayProvider
{
    private const CountryCode Sweden = CountryCode.SE;

    private readonly ConcurrentDictionary<int, Holiday[]> _holidaysByYear = new();

    public Holiday[] GetHolidays(int year)
    {
        return _holidaysByYear.GetOrAdd(year, ResolveHolidays);
    }

    private static Holiday[] ResolveHolidays(int year)
    {
        var publicHolidays = DateSystem.GetPublicHolidays(year, Sweden);
        var holidays = new List<Holiday>();

        foreach (var publicHoliday in publicHolidays)
        {
            holidays.Add(Holiday.Create(DateOnly.FromDateTime(publicHoliday.Date), publicHoliday.LocalName));
        }

        var ordered = holidays.ToArray();
        Array.Sort(ordered, (left, right) => left.Date.CompareTo(right.Date));

        return ordered;
    }
}
