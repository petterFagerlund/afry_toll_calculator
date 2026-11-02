using TollCalculator.Domain.Constants;
using TollCalculator.Domain.Enums;
using Xunit;

namespace TollCalculator.Domain.UnitTests.Constants;

public class ReportableVehicleTypesTests
{
    [Fact]
    public void All_IsEveryVehicleTypeExceptUnknown()
    {
        Assert.Equal(Enum.GetValues<VehicleType>().Length - 1, ReportableVehicleTypes.All.Length);
        Assert.DoesNotContain(VehicleType.Unknown, ReportableVehicleTypes.All);
    }

    [Fact]
    public void All_ContainsEveryTollFreeType()
    {
        foreach (var tollFree in TollFreeVehicleTypes.All)
        {
            Assert.Contains(tollFree, ReportableVehicleTypes.All);
        }
    }

    [Fact]
    public void Names_MatchTheEnumNames()
    {
        Assert.Equal(["Car", "Motorbike", "Tractor", "Emergency", "Diplomat", "Foreign", "Military"],
            ReportableVehicleTypes.Names());
    }
}
