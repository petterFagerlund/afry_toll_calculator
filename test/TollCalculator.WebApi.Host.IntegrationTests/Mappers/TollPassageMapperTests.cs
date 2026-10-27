using TollCalculator.WebApi.Host.Mappers.Implementations;
using Xunit;

namespace TollCalculator.WebApi.Host.IntegrationTests.Mappers;

public class TollPassageMapperTests
{
    private readonly TollPassageMapper _mapper = new();

    [Fact]
    public void ToResponse_CarriesThePassageId()
    {
        var passageId = Guid.CreateVersion7();

        var response = _mapper.ToResponse(passageId);

        Assert.Equal(passageId, response.TollPassageId);
    }
}
