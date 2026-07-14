using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tracksys.Modules.Alerting.Application.Dtos;
using Tracksys.Modules.Alerting.Application.Services;

namespace Tracksys.Modules.Alerting.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/alerting/alert-rules")]
public class AlertRulesController(
    AlertRuleQueryService alertRuleQueryService,
    AlertRuleCommandService alertRuleCommandService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await alertRuleQueryService.GetAllAsync(cancellationToken));

    [HttpPut("{id:int}/threshold")]
    public async Task<IActionResult> UpdateThreshold(int id, [FromBody] UpdateAlertRuleThresholdRequest request, CancellationToken cancellationToken)
    {
        var result = await alertRuleCommandService.UpdateThresholdAsync(id, request.Threshold, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    [HttpPut("{id:int}/toggle")]
    public async Task<IActionResult> Toggle(int id, [FromBody] ToggleAlertRuleRequest request, CancellationToken cancellationToken)
    {
        var result = await alertRuleCommandService.ToggleAsync(id, request.Enabled, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }
}
