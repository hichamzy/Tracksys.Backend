using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tracksys.Modules.Fleet.Application.Services;

namespace Tracksys.Modules.Fleet.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/fleet/vehicles")]
public class VehiclesController(VehicleQueryService vehicleQueryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await vehicleQueryService.GetAllAsync(cancellationToken));
}
