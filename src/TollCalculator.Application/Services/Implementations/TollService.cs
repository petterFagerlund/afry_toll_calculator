using TollCalculator.Application.Repositories;
using TollCalculator.Application.Results;
using TollCalculator.Domain.Models;

namespace TollCalculator.Application.Services.Implementations;

internal sealed class TollService : ITollService
{
    private static readonly TimeSpan ChargeWindowLength = TimeSpan.FromMinutes(60);

    private readonly ITollPassageRepository _tollPassageRepository;
    private readonly ITollScheduleRepository _tollScheduleRepository;
    private readonly ITollFeeService _tollFeeService;

    public TollService(
        ITollPassageRepository tollPassageRepository,
        ITollScheduleRepository tollScheduleRepository,
        ITollFeeService tollFeeService)
    {
        ArgumentNullException.ThrowIfNull(tollPassageRepository);
        ArgumentNullException.ThrowIfNull(tollScheduleRepository);
        ArgumentNullException.ThrowIfNull(tollFeeService);

        _tollPassageRepository = tollPassageRepository;
        _tollScheduleRepository = tollScheduleRepository;
        _tollFeeService = tollFeeService;
    }

    public async Task<ApplicationResult<DailyToll>> GetDailyTollAsync(
        string registrationNumber,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        var passages = await _tollPassageRepository.GetForDayAsync(registrationNumber, date, cancellationToken);
        if (passages.Length == 0)
        {
            return ApplicationResult<DailyToll>.Success(DailyToll.None());
        }

        var tollTariff = await _tollScheduleRepository.GetTariffAsync(cancellationToken);

        var chargeablePassages = PriceChargeablePassages(passages, tollTariff.Schedule);
        if (!chargeablePassages.IsSuccess)
        {
            return ApplicationResult<DailyToll>.Error(chargeablePassages.ErrorMessage!);
        }

        return ApplicationResult<DailyToll>.Success(ChargeOneDay(chargeablePassages.Value!, tollTariff.DailyCap));
    }

    private ApplicationResult<ChargedTollPassage[]> PriceChargeablePassages(
        TollPassage[] passages,
        TollSchedule schedule)
    {
        var chargeablePassages = new List<ChargedTollPassage>();

        foreach (var passage in passages.OrderBy(passage => passage.PassedAt))
        {
            var fee = _tollFeeService.CalculateFee(passage.VehicleType, passage.PassedAt, schedule);
            if (!fee.IsSuccess)
            {
                return ApplicationResult<ChargedTollPassage[]>.Error(fee.ErrorMessage!);
            }

            if (fee.Value == 0)
            {
                continue;
            }

            chargeablePassages.Add(new ChargedTollPassage(passage.PassedAt, fee.Value));
        }

        return ApplicationResult<ChargedTollPassage[]>.Success([.. chargeablePassages]);
    }

    private static DailyToll ChargeOneDay(ChargedTollPassage[] chargeablePassages, int dailyCap)
    {
        var chargedPassages = new List<ChargedTollPassage>();
        var totalFee = 0;
        var windowStart = 0;

        while (windowStart < chargeablePassages.Length && totalFee < dailyCap)
        {
            var windowEnd = EndOfChargeWindow(chargeablePassages, windowStart);
            var dearestPassage = chargeablePassages[windowStart..windowEnd].MaxBy(passage => passage.Amount)!;
            var amountToCharge = Math.Min(dearestPassage.Amount, dailyCap - totalFee);

            chargedPassages.Add(new ChargedTollPassage(dearestPassage.PassedAt, amountToCharge));
            totalFee += amountToCharge;
            windowStart = windowEnd;
        }

        return new DailyToll(totalFee, [.. chargedPassages]);
    }

    private static int EndOfChargeWindow(ChargedTollPassage[] chargeablePassages, int windowStart)
    {
        var windowOpenedAt = chargeablePassages[windowStart].PassedAt;
        var windowEnd = windowStart + 1;

        while (windowEnd < chargeablePassages.Length
               && chargeablePassages[windowEnd].PassedAt - windowOpenedAt < ChargeWindowLength)
        {
            windowEnd++;
        }

        return windowEnd;
    }
}
