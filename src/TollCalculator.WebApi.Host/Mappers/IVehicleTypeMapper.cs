using TollCalculator.Domain.Models;
using TollCalculator.WebApi.Models.V1.Responses;

namespace TollCalculator.WebApi.Host.Mappers;

public interface IVehicleTypeMapper
{
    VehicleTypeResponse[] ToResponses(VehicleTypeRule[] rules);
}
