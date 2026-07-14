using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tracksys.Modules.Alerting.Application.Services;

namespace Tracksys.Modules.Alerting.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/alerting/alert-types")]
public class AlertTypesController(AlertTypeQueryService alertTypeQueryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await alertTypeQueryService.GetAllAsync(cancellationToken));
}
