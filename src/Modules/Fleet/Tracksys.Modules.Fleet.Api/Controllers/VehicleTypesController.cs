using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tracksys.Modules.Fleet.Application.Services;

namespace Tracksys.Modules.Fleet.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/fleet/vehicle-types")]
public class VehicleTypesController(VehicleTypeQueryService vehicleTypeQueryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await vehicleTypeQueryService.GetAllAsync(cancellationToken));
}
