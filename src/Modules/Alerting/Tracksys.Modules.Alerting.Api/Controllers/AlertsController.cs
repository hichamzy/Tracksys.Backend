using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tracksys.Modules.Alerting.Application.Services;

namespace Tracksys.Modules.Alerting.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/alerting/alerts")]
public class AlertsController(AlertQueryService alertQueryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await alertQueryService.GetAllAsync(cancellationToken));
}
