using TollCalculator.Domain.Models;
using Xunit;

namespace TollCalculator.Domain.UnitTests.Models;

public class TollPassageTests
{
    private static readonly DateTimeOffset PassedAt = new(2026, 10, 23, 7, 15, 0, TimeSpan.FromHours(2));

    [Fact]
    public void Create_ValidInput_SetsProperties()
    {
        var passage = TollPassage.Create("ABC123", PassedAt);

        Assert.Equal("ABC123", passage.RegistrationNumber);
        Assert.Equal(PassedAt, passage.PassedAt);
    }

    [Fact]
    public void Create_ValidInput_GeneratesId()
    {
        var passage = TollPassage.Create("ABC123", PassedAt);

        Assert.NotEqual(Guid.Empty, passage.Id);
    }

    [Fact]
    public void Create_TwoPassages_HaveDifferentIds()
    {
        var first = TollPassage.Create("ABC123", PassedAt);
        var second = TollPassage.Create("ABC123", PassedAt);

        Assert.NotEqual(first.Id, second.Id);
    }

    [Theory]
    [InlineData("abc123", "ABC123")]
    [InlineData("  abc123  ", "ABC123")]
    public void Create_RegistrationNumberNeedsNormalising_TrimsAndUppercases(string input, string expected)
    {
        var passage = TollPassage.Create(input, PassedAt);

        Assert.Equal(expected, passage.RegistrationNumber);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_MissingRegistrationNumber_Throws(string? registrationNumber)
    {
        Assert.Throws<ArgumentException>(() => TollPassage.Create(registrationNumber!, PassedAt));
    }
}
