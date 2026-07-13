using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tracksys.Modules.Citizen.Application.Services;

namespace Tracksys.Modules.Citizen.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/citizen/complaints")]
public class ComplaintsController(ComplaintQueryService complaintQueryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await complaintQueryService.GetAllAsync(cancellationToken));
}
