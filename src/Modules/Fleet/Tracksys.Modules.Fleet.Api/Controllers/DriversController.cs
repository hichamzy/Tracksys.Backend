using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tracksys.Modules.Fleet.Application.Dtos;
using Tracksys.Modules.Fleet.Application.Services;

namespace Tracksys.Modules.Fleet.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/fleet/drivers")]
public class DriversController(DriverQueryService driverQueryService, DriverCommandService driverCommandService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await driverQueryService.GetAllAsync(cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDriverRequest request, CancellationToken cancellationToken)
    {
        var result = await driverCommandService.CreateAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(new { id = result.Value }) : BadRequest(new { error = result.Error });
    }

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeDriverStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await driverCommandService.ChangeStatusAsync(id, request.Status, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    [HttpPut("{id:int}/vehicle")]
    public async Task<IActionResult> AssignVehicle(int id, [FromBody] AssignDriverVehicleRequest request, CancellationToken cancellationToken)
    {
        var result = await driverCommandService.AssignVehicleAsync(id, request.VehicleId, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }
}
