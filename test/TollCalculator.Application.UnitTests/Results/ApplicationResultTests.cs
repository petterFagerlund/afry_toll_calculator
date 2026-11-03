using TollCalculator.Application.Results;

namespace TollCalculator.Application.UnitTests.Results;

public class ApplicationResultTests
{
    [Fact]
    public void Map_SuccessfulResult_ReturnsTheMappedValue()
    {
        var result = ApplicationResult<int>.Success(2).Map(value => value.ToString());

        Assert.True(result.IsSuccess);
        Assert.Equal("2", result.Value);
    }

    [Fact]
    public void Map_ErrorResult_KeepsTheErrorAndSkipsTheMapping()
    {
        var mapped = false;

        var result = ApplicationResult<int>.Error("broken").Map(value =>
        {
            mapped = true;
            return value;
        });

        Assert.Equal(ApplicationResultStatus.Error, result.Status);
        Assert.Equal("broken", result.ErrorMessage);
        Assert.False(mapped);
    }

    [Fact]
    public void Map_ConflictResult_KeepsTheConflictStatus()
    {
        var result = ApplicationResult<int>.Conflict("duplicate").Map(value => value.ToString());

        Assert.Equal(ApplicationResultStatus.Conflict, result.Status);
        Assert.Equal("duplicate", result.ErrorMessage);
    }
}
