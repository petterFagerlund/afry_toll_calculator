using TollCalculator.Domain.Models;

namespace TollCalculator.Domain.UnitTests;

internal static class TestSchedules
{
    public static TollSchedule Standard()
    {
        return TollSchedule.Create(
        [
            TollRateInterval.Create(new TimeOnly(6, 0), new TimeOnly(6, 29), 8),
            TollRateInterval.Create(new TimeOnly(6, 30), new TimeOnly(6, 59), 13),
            TollRateInterval.Create(new TimeOnly(7, 0), new TimeOnly(7, 59), 18),
            TollRateInterval.Create(new TimeOnly(8, 0), new TimeOnly(8, 29), 13),
            TollRateInterval.Create(new TimeOnly(8, 30), new TimeOnly(14, 59), 8),
            TollRateInterval.Create(new TimeOnly(15, 0), new TimeOnly(15, 29), 13),
            TollRateInterval.Create(new TimeOnly(15, 30), new TimeOnly(16, 59), 18),
            TollRateInterval.Create(new TimeOnly(17, 0), new TimeOnly(17, 59), 13),
            TollRateInterval.Create(new TimeOnly(18, 0), new TimeOnly(18, 29), 8)
        ]);
    }
}
