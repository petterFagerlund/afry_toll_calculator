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

    public DailyToll Calculate(Vehicle vehicle, TollPassage[] passages, TollSchedule schedule, int dailyCap)
    {
        ArgumentNullException.ThrowIfNull(vehicle);
        ArgumentNullException.ThrowIfNull(passages);
        ArgumentNullException.ThrowIfNull(schedule);

        if (dailyCap < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(dailyCap), dailyCap, "A daily cap cannot be negative.");
        }

        var chargeable = ChargeablePassages(vehicle, passages, schedule);

        if (chargeable.Length == 0)
        {
            return DailyToll.None();
        }

        var charged = new List<ChargedTollPassage>();
        var total = 0;
        var index = 0;

        while (index < chargeable.Length && total < dailyCap)
        {
            var window = HighestWithinWindow(chargeable, index);
            var amount = Math.Min(window.Amount, dailyCap - total);

            charged.Add(new ChargedTollPassage(window.PassedAt, amount));
            total += amount;
            index = window.NextIndex;
        }

        return new DailyToll(total, charged.ToArray());
    }

    private ChargedTollPassage[] ChargeablePassages(Vehicle vehicle, TollPassage[] passages, TollSchedule schedule)
    {
        var chargeable = new List<ChargedTollPassage>();

        foreach (var passage in passages)
        {
            var fee = _tollFeeService.CalculateFee(vehicle, passage.PassedAt, schedule);

            if (fee > 0)
            {
                chargeable.Add(new ChargedTollPassage(passage.PassedAt, fee));
            }
        }

        var ordered = chargeable.ToArray();
        Array.Sort(ordered, (left, right) => left.PassedAt.CompareTo(right.PassedAt));

        return ordered;
    }

    private static TollWindow HighestWithinWindow(ChargedTollPassage[] chargeable, int startIndex)
    {
        var windowStart = chargeable[startIndex].PassedAt;
        var highest = chargeable[startIndex];
        var next = startIndex + 1;

        while (next < chargeable.Length && chargeable[next].PassedAt - windowStart < ChargeWindow)
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
