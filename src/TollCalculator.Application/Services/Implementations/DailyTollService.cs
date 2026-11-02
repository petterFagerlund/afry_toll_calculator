using TollCalculator.Application.Repositories;
using TollCalculator.Application.Results;
using TollCalculator.Domain.Models;

namespace TollCalculator.Application.Services.Implementations;

internal sealed class DailyTollService : IDailyTollService
{
    private static readonly TimeSpan ChargeWindowLength = TimeSpan.FromMinutes(60);

    private sealed record ChargeWindow(DateTimeOffset PassedAt, int Amount, int IndexAfterWindow);

    private readonly ITollFeeService _tollFeeService;
    private readonly ITollScheduleRepository _tollScheduleRepository;

    public DailyTollService(ITollFeeService tollFeeService, ITollScheduleRepository tollScheduleRepository)
    {
        ArgumentNullException.ThrowIfNull(tollFeeService);
        ArgumentNullException.ThrowIfNull(tollScheduleRepository);

        _tollFeeService = tollFeeService;
        _tollScheduleRepository = tollScheduleRepository;
    }

    public async Task<ApplicationResult<DailyToll>> Calculate(TollPassage[] passages, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(passages);

        var tollTariff = await _tollScheduleRepository.GetTariffAsync(cancellationToken);

        var chargeablePassagesResult = ChargeablePassagesInTimeOrder(passages, tollTariff.Schedule);
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
        var totalFee = ChargeOneDay(chargeablePassages, tollTariff.DailyCap, chargedPassages);

        return ApplicationResult<DailyToll>.Success(new DailyToll(totalFee, [.. chargedPassages]));
    }

    private ApplicationResult<ChargedTollPassage[]> ChargeablePassagesInTimeOrder(
        TollPassage[] passages,
        TollSchedule schedule)
    {
        var chargeablePassages = new List<ChargedTollPassage>();

        foreach (var passage in passages)
        {
            var tollFee = _tollFeeService.CalculateFee(passage.VehicleType, passage.PassedAt, schedule);
            if (!tollFee.IsSuccess)
            {
                return ApplicationResult<ChargedTollPassage[]>.Error(tollFee.ErrorMessage!);
            }

            if (tollFee.Value > 0)
            {
                chargeablePassages.Add(new ChargedTollPassage(passage.PassedAt, tollFee.Value));
            }
        }

        return ApplicationResult<ChargedTollPassage[]>.Success(
            [.. chargeablePassages.OrderBy(chargeablePassage => chargeablePassage.PassedAt)]);
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