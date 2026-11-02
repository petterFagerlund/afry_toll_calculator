using TollCalculator.Domain.Models;

namespace TollCalculator.Application.UnitTests;

internal static class TestSchedules
{
    public static TollSchedule Standard()
    {
        return TollSchedule.Create(
        [
            TollRateInterval.Create(new TimeOnly(6, 0), new TimeOnly(6, 30), 8),
            TollRateInterval.Create(new TimeOnly(6, 30), new TimeOnly(7, 0), 13),
            TollRateInterval.Create(new TimeOnly(7, 0), new TimeOnly(8, 0), 18),
            TollRateInterval.Create(new TimeOnly(8, 0), new TimeOnly(8, 30), 13),
            TollRateInterval.Create(new TimeOnly(8, 30), new TimeOnly(15, 0), 8),
            TollRateInterval.Create(new TimeOnly(15, 0), new TimeOnly(15, 30), 13),
            TollRateInterval.Create(new TimeOnly(15, 30), new TimeOnly(17, 0), 18),
            TollRateInterval.Create(new TimeOnly(17, 0), new TimeOnly(18, 0), 13),
            TollRateInterval.Create(new TimeOnly(18, 0), new TimeOnly(18, 30), 8)
        ]);
    }
}
