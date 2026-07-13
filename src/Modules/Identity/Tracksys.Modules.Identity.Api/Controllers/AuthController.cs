using Microsoft.AspNetCore.Mvc;
using Tracksys.Modules.Identity.Application.Dtos;
using Tracksys.Modules.Identity.Application.Services;

namespace Tracksys.Modules.Identity.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AuthService authService) : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : Unauthorized(new { error = result.Error });
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.RefreshAsync(request, HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : Unauthorized(new { error = result.Error });
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest request, CancellationToken cancellationToken)
    {
        await authService.RevokeAsync(request.RefreshToken, cancellationToken);
        return NoContent();
    }
}
