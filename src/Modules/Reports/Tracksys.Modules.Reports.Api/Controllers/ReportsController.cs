using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tracksys.Modules.Reports.Application.Dtos;
using Tracksys.Modules.Reports.Application.Services;
using Tracksys.Shared.Infrastructure.Auth;

namespace Tracksys.Modules.Reports.Api.Controllers;

[ApiController]
[Authorize]
[RequireModule("report")]
[Route("api/reports")]
public class ReportsController(ReportQueryService reportQueryService, ReportCommandService reportCommandService) : ControllerBase
{
    [HttpGet("kpis")]
    public async Task<IActionResult> GetKpis(CancellationToken cancellationToken) =>
        Ok(await reportQueryService.GetKpisAsync(cancellationToken));

    [HttpGet("distance-series")]
    public async Task<IActionResult> GetDistanceSeries(CancellationToken cancellationToken) =>
        Ok(await reportQueryService.GetDistanceSeriesAsync(cancellationToken));

    [HttpGet("resolution-series")]
    public async Task<IActionResult> GetResolutionSeries(CancellationToken cancellationToken) =>
        Ok(await reportQueryService.GetResolutionSeriesAsync(cancellationToken));

    [HttpGet("types")]
    public async Task<IActionResult> GetReportTypes(CancellationToken cancellationToken) =>
        Ok(await reportQueryService.GetReportTypesAsync(cancellationToken));

    [HttpGet("saved")]
    public async Task<IActionResult> GetSaved(CancellationToken cancellationToken) =>
        Ok(await reportQueryService.GetSavedReportsAsync(cancellationToken));

    [HttpPost("saved")]
    public async Task<IActionResult> Save([FromBody] CreateSavedReportRequest request, CancellationToken cancellationToken)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        var result = await reportCommandService.SaveAsync(request, userId, cancellationToken);
        return result.IsSuccess ? Ok(new { id = result.Value }) : BadRequest(new { error = result.Error });
    }
}
