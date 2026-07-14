using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tracksys.Modules.Identity.Application.Dtos;
using Tracksys.Modules.Identity.Application.Services;

namespace Tracksys.Modules.Identity.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/identity/users")]
public class UsersController(UserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await userService.GetAllAsync());

    [HttpPost]
    [Authorize(Roles = "Administrateur")]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var result = await userService.CreateAsync(request);
        return result.IsSuccess ? Ok(new { id = result.Value }) : BadRequest(new { error = result.Error });
    }

    [HttpPut("{id}/role")]
    [Authorize(Roles = "Administrateur")]
    public async Task<IActionResult> ChangeRole(string id, [FromBody] ChangeUserRoleRequest request)
    {
        var result = await userService.ChangeRoleAsync(id, request.Role);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    [HttpPut("{id}/active")]
    [Authorize(Roles = "Administrateur")]
    public async Task<IActionResult> SetActive(string id, [FromBody] SetUserActiveRequest request)
    {
        var result = await userService.SetActiveAsync(id, request.IsActive);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }
}
