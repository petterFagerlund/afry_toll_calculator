using TollCalculator.Domain.Models;
using Xunit;

namespace TollCalculator.Domain.UnitTests.Models;

public class HolidayTests
{
    [Fact]
    public void Create_ValidInput_SetsProperties()
    {
        var holiday = Holiday.Create(new DateOnly(2026, 1, 1), "Nyårsdagen");

        Assert.Equal(new DateOnly(2026, 1, 1), holiday.Date);
        Assert.Equal("Nyårsdagen", holiday.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_MissingName_Throws(string? name)
    {
        Assert.Throws<ArgumentException>(() => Holiday.Create(new DateOnly(2026, 1, 1), name!));
    }
}
