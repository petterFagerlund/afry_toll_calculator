using TollCalculator.WebApi.Models.V1.Responses;

namespace TollCalculator.WebApi.Host.Mappers.Implementations;

internal sealed class TollPassageMapper : ITollPassageMapper
{
    public RecordTollPassageResponse ToResponse(Guid tollPassageId)
    {
        return new RecordTollPassageResponse { TollPassageId = tollPassageId };
    }
}
