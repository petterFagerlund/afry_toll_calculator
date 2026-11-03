using System.Text.Json.Nodes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using TollCalculator.Domain.Constants;
using TollCalculator.Domain.Enums;
using TollCalculator.WebApi.Models.V1.Requests;

namespace TollCalculator.WebApi.Host.OpenApi.Transformers;

/// <summary>
/// Publishes the vehicle types the API accepts on the <see cref="TollPassageToRecord.VehicleType"/> string.
/// </summary>
/// <remarks>
/// The contract assembly deliberately declares no enum of its own, so the accepted values are read from the
/// domain at start-up instead of being restated in the contract. The documented list therefore cannot drift
/// from what <see cref="VehicleTypeParser"/> actually accepts.
/// </remarks>
internal sealed class VehicleTypeSchemaTransformer : IOpenApiSchemaTransformer
{
    private const string VehicleTypeProperty = "vehicleType";

    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(schema);
        ArgumentNullException.ThrowIfNull(context);

        if (context.JsonTypeInfo.Type != typeof(TollPassageToRecord))
        {
            return Task.CompletedTask;
        }

        if (schema.Properties?.TryGetValue(VehicleTypeProperty, out var property) != true
            || property is not OpenApiSchema vehicleType)
        {
            return Task.CompletedTask;
        }

        vehicleType.Enum = AcceptedVehicleTypes();
        vehicleType.Description = "The vehicle type the roadside equipment reported.";

        return Task.CompletedTask;
    }

    private static List<JsonNode> AcceptedVehicleTypes()
    {
        return ReportableVehicleTypes.Names().Select(name => (JsonNode)JsonValue.Create(name)!).ToList();
    }
}
