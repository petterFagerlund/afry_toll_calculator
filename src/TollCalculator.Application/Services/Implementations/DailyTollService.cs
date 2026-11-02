using TollCalculator.Application.Results;
using TollCalculator.Domain.Constants;
using TollCalculator.Domain.Enums;
using TollCalculator.Domain.Models;

namespace TollCalculator.Application.Services.Implementations;

public sealed class DailyTollService : IDailyTollService
{
    private static readonly TimeSpan ChargeWindowLength = TimeSpan.FromMinutes(60);
    private sealed record ChargeWindow(DateTimeOffset PassedAt, int Amount, int IndexAfterWindow);
    private readonly ITollFeeService _tollFeeService;

    public DailyTollService(ITollFeeService tollFeeService)
    {
        ArgumentNullException.ThrowIfNull(tollFeeService);
        _tollFeeService = tollFeeService;
    }

    public ApplicationResult<DailyToll> Calculate(
        VehicleType vehicleType,
        DateTimeOffset[] timeStamps,
        TollTariff tariff)
    {
        ArgumentNullException.ThrowIfNull(timeStamps);
        ArgumentNullException.ThrowIfNull(tariff);

        var chargeablePassagesResult = ChargeablePassagesInTimeOrder(vehicleType, timeStamps, tariff.Schedule);
        if (!chargeablePassagesResult.IsSuccess)
        {
            return ApplicationResult<DailyToll>.Error(chargeablePassagesResult.ErrorMessage!);
        }

        var chargeablePassages = chargeablePassagesResult.Value!;
        if (chargeablePassages.Length == 0)
        {
            return ApplicationResult<DailyToll>.Success(DailyToll.None());
        }

        var chargedPassages = new List<ChargedTollPassage>();
        var totalFee = 0;

        foreach (var passagesOnOneDay in GroupByStockholmDay(chargeablePassages))
        {
            totalFee += ChargeOneDay(passagesOnOneDay, tariff.DailyCap, chargedPassages);
        }

        return ApplicationResult<DailyToll>.Success(new DailyToll(totalFee, [.. chargedPassages]));
    }

    private ApplicationResult<ChargedTollPassage[]> ChargeablePassagesInTimeOrder(
        VehicleType vehicleType,
        DateTimeOffset[] timeStamps,
        TollSchedule schedule)
    {
        var chargeablePassages = new List<ChargedTollPassage>();

        foreach (var passedAt in timeStamps)
        {
            var tollFee = _tollFeeService.CalculateFee(vehicleType, passedAt, schedule);
            if (!tollFee.IsSuccess)
            {
                return ApplicationResult<ChargedTollPassage[]>.Error(tollFee.ErrorMessage!);
            }

            if (tollFee.Value > 0)
            {
                chargeablePassages.Add(new ChargedTollPassage(passedAt, tollFee.Value));
            }
        }

        return ApplicationResult<ChargedTollPassage[]>.Success(
            [.. chargeablePassages.OrderBy(chargeablePassage => chargeablePassage.PassedAt)]);
    }

    private static ChargedTollPassage[][] GroupByStockholmDay(ChargedTollPassage[] passagesInTimeOrder)
    {
        var days = new List<ChargedTollPassage[]>();
        var passagesOnCurrentDay = new List<ChargedTollPassage>();
        var currentDay = TollTimeZone.DateOf(passagesInTimeOrder[0].PassedAt);

        foreach (var passage in passagesInTimeOrder)
        {
            var dayOfPassage = TollTimeZone.DateOf(passage.PassedAt);

            if (dayOfPassage != currentDay)
            {
                days.Add([.. passagesOnCurrentDay]);
                passagesOnCurrentDay.Clear();
                currentDay = dayOfPassage;
            }

            passagesOnCurrentDay.Add(passage);
        }

        days.Add([.. passagesOnCurrentDay]);

        return [.. days];
    }

    private static int ChargeOneDay(
        ChargedTollPassage[] passagesOnOneDay,
        int dailyCap,
        List<ChargedTollPassage> chargedPassages)
    {
        var feeChargedSoFar = 0;

        // The index advances by a whole charge window, which is only known once the body has measured it.
        for (var windowStartIndex = 0; windowStartIndex < passagesOnOneDay.Length && feeChargedSoFar < dailyCap;)
        {
            var window = DearestPassageInChargeWindow(passagesOnOneDay, windowStartIndex);
            var amountToCharge = Math.Min(window.Amount, dailyCap - feeChargedSoFar);

            chargedPassages.Add(new ChargedTollPassage(window.PassedAt, amountToCharge));
            feeChargedSoFar += amountToCharge;
            windowStartIndex = window.IndexAfterWindow;
        }

        return feeChargedSoFar;
    }

    private static ChargeWindow DearestPassageInChargeWindow(
        ChargedTollPassage[] passagesOnOneDay,
        int windowStartIndex)
    {
        var windowOpenedAt = passagesOnOneDay[windowStartIndex].PassedAt;
        var dearestPassage = passagesOnOneDay[windowStartIndex];
        var indexAfterWindow = windowStartIndex + 1;

        for (var index = windowStartIndex + 1; index < passagesOnOneDay.Length; index++)
        {
            if (passagesOnOneDay[index].PassedAt - windowOpenedAt >= ChargeWindowLength)
            {
                break;
            }

            if (passagesOnOneDay[index].Amount > dearestPassage.Amount)
            {
                dearestPassage = passagesOnOneDay[index];
            }

            indexAfterWindow = index + 1;
        }

        return new ChargeWindow(dearestPassage.PassedAt, dearestPassage.Amount, indexAfterWindow);
    }
}
