using TollCalculator.Application.Results;
using TollCalculator.Domain.Constants;
using TollCalculator.Domain.Enums;
using TollCalculator.Domain.Models;

namespace TollCalculator.Application.Services.Implementations;

public sealed class DailyTollService : IDailyTollService
{
    private static readonly TimeSpan ChargeWindow = TimeSpan.FromMinutes(60);
    private sealed record TollWindow(DateTimeOffset PassedAt, int Amount, int NextIndex);
    private readonly ITollFeeService _tollFeeService;

    public DailyTollService(ITollFeeService tollFeeService)
    {
        ArgumentNullException.ThrowIfNull(tollFeeService);
        _tollFeeService = tollFeeService;
    }

    public ApplicationResult<DailyToll> Calculate(
        VehicleType vehicleType,
        DateTimeOffset[] passedAt,
        TollTariff tariff)
    {
        ArgumentNullException.ThrowIfNull(passedAt);
        ArgumentNullException.ThrowIfNull(tariff);

        var chargeablePassages = ChargeablePassages(vehicleType, passedAt, tariff.Schedule);

        if (!chargeablePassages.IsSuccess)
        {
            return ApplicationResult<DailyToll>.Error(chargeablePassages.ErrorMessage!);
        }

        var chargeable = chargeablePassages.Value!;

        if (chargeable.Length == 0)
        {
            return ApplicationResult<DailyToll>.Success(DailyToll.None());
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

        return ApplicationResult<DailyToll>.Success(new DailyToll(total, charged.ToArray()));
    }

    private ApplicationResult<ChargedTollPassage[]> ChargeablePassages(
        VehicleType vehicleType,
        DateTimeOffset[] passedAt,
        TollSchedule schedule)
    {
        var chargeable = new List<ChargedTollPassage>();

        foreach (var moment in passedAt)
        {
            var fee = _tollFeeService.CalculateFee(vehicleType, moment, schedule);

            if (!fee.IsSuccess)
            {
                return ApplicationResult<ChargedTollPassage[]>.Error(fee.ErrorMessage!);
            }

            if (fee.Value > 0)
            {
                chargeable.Add(new ChargedTollPassage(moment, fee.Value));
            }
        }

        var ordered = chargeable.ToArray();
        Array.Sort(ordered, (left, right) => left.PassedAt.CompareTo(right.PassedAt));

        return ApplicationResult<ChargedTollPassage[]>.Success(ordered);
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
}
