using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tracksys.Modules.Tenancy.Application.Dtos;
using Tracksys.Modules.Tenancy.Application.Services;

namespace Tracksys.Modules.Tenancy.Api.Controllers;

[ApiController]
[Route("api/tenancy/cities")]
public class CitiesController(
    CityQueryService cityQueryService,
    CityCommandService cityCommandService) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await cityQueryService.GetAllAsync(cancellationToken));

    /// <summary>Accessible à tout utilisateur authentifié — permet à un compte non-SuperAdmin
    /// de résoudre le nom de sa propre ville (AuthResponse ne renvoie que le CityId).</summary>
    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var city = await cityQueryService.GetByIdAsync(id, cancellationToken);
        return city is null ? NotFound() : Ok(city);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateCityRequest request, CancellationToken cancellationToken)
    {
        var result = await cityCommandService.CreateAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCityRequest request, CancellationToken cancellationToken)
    {
        var result = await cityCommandService.UpdateAsync(id, request, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }
}
