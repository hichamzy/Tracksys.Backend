using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tracksys.Modules.Ingestion.Application.Abstractions;
using Tracksys.Shared.Kernel.Auth;

namespace Tracksys.Modules.Ingestion.Api.Controllers;

/// <summary>
/// Public par design : carte live et historique GPS consultables sans compte
/// (décision produit). Ne pas ajouter [Authorize] ici sans revalider ce choix.
/// Si un JWT valide est fourni (le front est toujours connecté), les résultats sont filtrés
/// par la ville du token via ICurrentTenantAccessor — sans JWT ou pour un SuperAdmin, aucun
/// filtre n'est appliqué (comportement legacy inchangé).
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/positions")]
public class PositionsController(IPositionQueryService positionQueryService, ICurrentTenantAccessor tenant) : ControllerBase
{
    [HttpGet("live")]
    public async Task<IActionResult> GetLive(CancellationToken cancellationToken)
    {
        Guid? cityId = tenant.IsSuperAdmin ? null : tenant.CityId;
        return Ok(await positionQueryService.GetLiveAsync(cityId, cancellationToken));
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] string deviceId, [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(deviceId)) return BadRequest("deviceId requis.");
        if (to <= from) return BadRequest("to doit être postérieur à from.");

        DateTime fromUtc = DateTime.SpecifyKind(from, DateTimeKind.Utc);
        DateTime toUtc = DateTime.SpecifyKind(to, DateTimeKind.Utc);

        Guid? cityId = tenant.IsSuperAdmin ? null : tenant.CityId;
        return Ok(await positionQueryService.GetHistoryAsync(deviceId, fromUtc, toUtc, cityId, cancellationToken));
    }
}
