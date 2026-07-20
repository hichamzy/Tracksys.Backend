using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tracksys.Modules.Alerting.Application.Services;
using Tracksys.Shared.Infrastructure.Auth;

namespace Tracksys.Modules.Alerting.Api.Controllers;

[ApiController]
[Authorize]
[RequireModule("alerts")]
[Route("api/alerting/alerts")]
public class AlertsController(AlertQueryService alertQueryService, AlertCommandService alertCommandService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await alertQueryService.GetAllAsync(cancellationToken));

    [HttpPut("{id:long}/read")]
    public async Task<IActionResult> MarkAsRead(long id, CancellationToken cancellationToken)
    {
        var result = await alertCommandService.MarkAsReadAsync(id, CurrentUserId, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
    {
        var result = await alertCommandService.MarkAllAsReadAsync(CurrentUserId, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
}
