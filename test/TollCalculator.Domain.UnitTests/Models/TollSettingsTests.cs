using TollCalculator.Domain.Models;
using Xunit;

namespace TollCalculator.Domain.UnitTests.Models;

public class TollSettingsTests
{
    [Fact]
    public void Create_ValidDailyCap_SetsProperty()
    {
        var settings = TollSettings.Create(60);

        Assert.Equal(60, settings.DailyCap);
    }

    [Fact]
    public void Create_NegativeDailyCap_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => TollSettings.Create(-1));
    }
}
