using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tracksys.Modules.Ingestion.Application.Abstractions;

namespace Tracksys.Modules.Ingestion.Api.Controllers;

/// <summary>
/// Public par design : carte live et historique GPS consultables sans compte
/// (décision produit). Ne pas ajouter [Authorize] ici sans revalider ce choix.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/positions")]
public class PositionsController(IPositionQueryService positionQueryService) : ControllerBase
{
    [HttpGet("live")]
    public async Task<IActionResult> GetLive(CancellationToken cancellationToken) =>
        Ok(await positionQueryService.GetLiveAsync(cancellationToken));

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] string deviceId, [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(deviceId)) return BadRequest("deviceId requis.");
        if (to <= from) return BadRequest("to doit être postérieur à from.");

        DateTime fromUtc = DateTime.SpecifyKind(from, DateTimeKind.Utc);
        DateTime toUtc = DateTime.SpecifyKind(to, DateTimeKind.Utc);

        return Ok(await positionQueryService.GetHistoryAsync(deviceId, fromUtc, toUtc, cancellationToken));
    }
}
