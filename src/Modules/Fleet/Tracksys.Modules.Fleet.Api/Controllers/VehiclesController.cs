using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tracksys.Modules.Fleet.Application.Dtos;
using Tracksys.Modules.Fleet.Application.Services;
using Tracksys.Shared.Infrastructure.Auth;

namespace Tracksys.Modules.Fleet.Api.Controllers;

[ApiController]
[Authorize]
[RequireModule("fleet")]
[Route("api/fleet/vehicles")]
public class VehiclesController(VehicleQueryService vehicleQueryService, VehicleCommandService vehicleCommandService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await vehicleQueryService.GetAllAsync(cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVehicleRequest request, CancellationToken cancellationToken)
    {
        var result = await vehicleCommandService.CreateAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(new { id = result.Value }) : BadRequest(new { error = result.Error });
    }

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeVehicleStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await vehicleCommandService.ChangeStatusAsync(id, request.Status, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    [HttpPut("{id:int}/driver")]
    public async Task<IActionResult> AssignDriver(int id, [FromBody] AssignVehicleDriverRequest request, CancellationToken cancellationToken)
    {
        var result = await vehicleCommandService.AssignDriverAsync(id, request.DriverId, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }
}
