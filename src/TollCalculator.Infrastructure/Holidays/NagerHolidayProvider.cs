using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Nager.Date;
using TollCalculator.Domain.Models;
using TollCalculator.Domain.Services;

namespace TollCalculator.Infrastructure.Holidays;

internal sealed class NagerHolidayProvider : IHolidayProvider
{
    private const CountryCode Sweden = CountryCode.SE;

    private readonly ConcurrentDictionary<int, Holiday[]> _holidaysByYear = new();
    private readonly ILogger<NagerHolidayProvider> _logger;

    public NagerHolidayProvider(ILogger<NagerHolidayProvider> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
    }

    public Holiday[] GetHolidays(int year)
    {
        return _holidaysByYear.GetOrAdd(year, ResolveHolidays);
    }

    private Holiday[] ResolveHolidays(int year)
    {
        var publicHolidays = DateSystem.GetPublicHolidays(year, Sweden);
        var holidays = new List<Holiday>();

        foreach (var publicHoliday in publicHolidays)
        {
            holidays.Add(Holiday.Create(DateOnly.FromDateTime(publicHoliday.Date), publicHoliday.LocalName));
        }

        var ordered = holidays.ToArray();
        Array.Sort(ordered, (left, right) => left.Date.CompareTo(right.Date));

        _logger.LogDebug("Resolved {HolidayCount} Swedish public holidays for {Year}.", ordered.Length, year);

        return ordered;
    }
}
