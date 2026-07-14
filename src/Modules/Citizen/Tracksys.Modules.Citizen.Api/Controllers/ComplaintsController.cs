using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tracksys.Modules.Citizen.Application.Dtos;
using Tracksys.Modules.Citizen.Application.Services;

namespace Tracksys.Modules.Citizen.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/citizen/complaints")]
public class ComplaintsController(
    ComplaintQueryService complaintQueryService,
    ComplaintCommandService complaintCommandService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await complaintQueryService.GetAllAsync(cancellationToken));

    [HttpGet("category-breakdown")]
    public async Task<IActionResult> GetCategoryBreakdown(CancellationToken cancellationToken) =>
        Ok(await complaintQueryService.GetCategoryBreakdownAsync(cancellationToken));

    [HttpPut("{id:int}/assign")]
    public async Task<IActionResult> Assign(int id, [FromBody] AssignComplaintRequest request, CancellationToken cancellationToken)
    {
        var result = await complaintCommandService.AssignVehicleAsync(id, request.VehicleId, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    [HttpPut("{id:int}/resolve")]
    public async Task<IActionResult> Resolve(int id, [FromBody] ResolveComplaintRequest request, CancellationToken cancellationToken)
    {
        var result = await complaintCommandService.ResolveAsync(id, request.PhotoAfterUrl, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }
}
