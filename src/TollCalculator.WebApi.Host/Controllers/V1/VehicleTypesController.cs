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
    private readonly IVehicleTypeMapper _vehicleTypeMapper;

    public VehicleTypesController(IVehicleTypeService vehicleTypeService, IVehicleTypeMapper vehicleTypeMapper)
    {
        ArgumentNullException.ThrowIfNull(vehicleTypeService);
        ArgumentNullException.ThrowIfNull(vehicleTypeMapper);

        _vehicleTypeService = vehicleTypeService;
        _vehicleTypeMapper = vehicleTypeMapper;
    }

    /// <summary>Returns every vehicle type with the flag that says whether it is toll free.</summary>
    [HttpGet("api/v{version:apiVersion}/vehicle-types")]
    [ProducesResponseType<VehicleTypeResponse[]>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VehicleTypeResponse[]>> GetVehicleTypes(CancellationToken cancellationToken)
    {
        var result = await _vehicleTypeService.GetVehicleTypeRulesAsync(cancellationToken);

        if (!result.IsSuccess)
        {
            return Failure(result);
        }

        return Ok(_vehicleTypeMapper.ToResponses(result.Value!));
    }
}
