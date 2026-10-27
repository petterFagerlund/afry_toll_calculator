using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TollCalculator.Application.Services;
using TollCalculator.WebApi.Host.Mappers;
using TollCalculator.WebApi.Models.V1.Responses;

namespace TollCalculator.WebApi.Host.Controllers.V1;

[ApiVersion("1.0")]
public sealed class VehicleTypesController : ApiControllerBase
{
    private readonly IVehicleTypeService _vehicleTypeService;

    public VehicleTypesController(IVehicleTypeService vehicleTypeService)
    {
        ArgumentNullException.ThrowIfNull(vehicleTypeService);

        _vehicleTypeService = vehicleTypeService;
    }

    /// <summary>Returns every vehicle type with the flag that says whether it is toll free.</summary>
    [HttpGet("api/v{version:apiVersion}/vehicle-types")]
    [ProducesResponseType<VehicleTypeResponse[]>(StatusCodes.Status200OK)]
    public async Task<ActionResult<VehicleTypeResponse[]>> GetVehicleTypes(CancellationToken cancellationToken)
    {
        var rules = await _vehicleTypeService.GetVehicleTypeRulesAsync(cancellationToken);

        return Ok(VehicleTypeMapper.ToResponses(rules));
    }
}
