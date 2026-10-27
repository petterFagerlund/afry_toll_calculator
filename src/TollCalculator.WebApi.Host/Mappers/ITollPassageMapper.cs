using TollCalculator.WebApi.Models.V1.Responses;

namespace TollCalculator.WebApi.Host.Mappers;

public interface ITollPassageMapper
{
    RecordTollPassageResponse ToResponse(Guid tollPassageId);
}
