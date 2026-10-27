using TollCalculator.Domain.Constants;
using TollCalculator.Domain.Models;

namespace TollCalculator.Domain.Services.Implementations;

public sealed class DailyTollService : IDailyTollService
{
    private static readonly TimeSpan ChargeWindow = TimeSpan.FromMinutes(60);

    private readonly ITollFeeService _tollFeeService;

    public DailyTollService(ITollFeeService tollFeeService)
    {
        ArgumentNullException.ThrowIfNull(tollFeeService);

        _tollFeeService = tollFeeService;
    }

    public DailyToll Calculate(Vehicle vehicle, TollPassage[] passages, TollTariff tariff, Holiday[] holidays)
    {
        ArgumentNullException.ThrowIfNull(vehicle);
        ArgumentNullException.ThrowIfNull(passages);
        ArgumentNullException.ThrowIfNull(tariff);
        ArgumentNullException.ThrowIfNull(holidays);

        var chargeable = ChargeablePassages(vehicle, passages, tariff.Schedule, holidays);

        if (chargeable.Length == 0)
        {
            return DailyToll.None();
        }

        var charged = new List<ChargedTollPassage>();
        var total = 0;
        var index = 0;

        while (index < chargeable.Length)
        {
            var dayEnd = EndOfDay(chargeable, index);

            total += ChargeDay(chargeable, index, dayEnd, tariff.DailyCap, charged);
            index = dayEnd;
        }

        return new DailyToll(total, charged.ToArray());
    }

    private ChargedTollPassage[] ChargeablePassages(
        Vehicle vehicle,
        TollPassage[] passages,
        TollSchedule schedule,
        Holiday[] holidays)
    {
        var chargeable = new List<ChargedTollPassage>();

        foreach (var passage in passages)
        {
            var fee = _tollFeeService.CalculateFee(vehicle, passage.PassedAt, schedule, holidays);

            if (fee > 0)
            {
                chargeable.Add(new ChargedTollPassage(passage.PassedAt, fee));
            }
        }

        var ordered = chargeable.ToArray();
        Array.Sort(ordered, (left, right) => left.PassedAt.CompareTo(right.PassedAt));

        return ordered;
    }

    private static int EndOfDay(ChargedTollPassage[] chargeable, int startIndex)
    {
        var day = TollTimeZone.DateOf(chargeable[startIndex].PassedAt);
        var next = startIndex + 1;

        while (next < chargeable.Length && TollTimeZone.DateOf(chargeable[next].PassedAt) == day)
        {
            next++;
        }

        return next;
    }

    private static int ChargeDay(
        ChargedTollPassage[] chargeable,
        int startIndex,
        int endIndex,
        int dailyCap,
        List<ChargedTollPassage> charged)
    {
        var dayTotal = 0;
        var index = startIndex;

        while (index < endIndex && dayTotal < dailyCap)
        {
            var window = HighestWithinWindow(chargeable, index, endIndex);
            var amount = Math.Min(window.Amount, dailyCap - dayTotal);

            charged.Add(new ChargedTollPassage(window.PassedAt, amount));
            dayTotal += amount;
            index = window.NextIndex;
        }

        return dayTotal;
    }

    private static TollWindow HighestWithinWindow(ChargedTollPassage[] chargeable, int startIndex, int endIndex)
    {
        var windowStart = chargeable[startIndex].PassedAt;
        var highest = chargeable[startIndex];
        var next = startIndex + 1;

        while (next < endIndex && chargeable[next].PassedAt - windowStart < ChargeWindow)
        {
            if (chargeable[next].Amount > highest.Amount)
            {
                highest = chargeable[next];
            }

            next++;
        }

        return new TollWindow(highest.PassedAt, highest.Amount, next);
    }

    private sealed record TollWindow(DateTimeOffset PassedAt, int Amount, int NextIndex);
}
