using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Nager.Date;
using TollCalculator.Application.Providers;
using TollCalculator.Application.Results;
using TollCalculator.Domain.Models;

namespace TollCalculator.Infrastructure.Providers;

internal sealed class HolidayProvider : IHolidayProvider
{
    private const CountryCode Sweden = CountryCode.SE;

    private readonly ConcurrentDictionary<int, Holiday[]> _holidaysByYear = new();
    private readonly ILogger<HolidayProvider> _logger;

    public HolidayProvider(ILogger<HolidayProvider> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
    }

    public ApplicationResult<Holiday[]> GetHolidays(int year)
    {
        if (_holidaysByYear.TryGetValue(year, out var cached))
        {
            return ApplicationResult<Holiday[]>.Success(cached);
        }

        try
        {
            var resolved = ResolveHolidays(year);

            _holidaysByYear[year] = resolved;

            return ApplicationResult<Holiday[]>.Success(resolved);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Resolving the Swedish public holidays for {Year} failed.", year);

            return ApplicationResult<Holiday[]>.Error(
                $"The Swedish public holidays for {year} could not be resolved.");
        }
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

        return ordered;
    }
}