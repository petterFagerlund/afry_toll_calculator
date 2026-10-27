using TollCalculator.Domain.Models;
using TollCalculator.WebApi.Models.V1.Responses;

namespace TollCalculator.WebApi.Host.Mappers;

internal static class VehicleTypeMapper
{
    public static VehicleTypeResponse[] ToResponses(VehicleTypeRule[] rules)
    {
        ArgumentNullException.ThrowIfNull(rules);

        var responses = new VehicleTypeResponse[rules.Length];

        for (var index = 0; index < rules.Length; index++)
        {
            responses[index] = new VehicleTypeResponse
            {
                VehicleType = rules[index].VehicleType.ToString(),
                IsTollFree = rules[index].IsTollFree,
            };
        }

        return responses;
    }
}
