using Microsoft.AspNetCore.Identity;
using Tracksys.Modules.Identity.Application.Dtos;
using Tracksys.Modules.Identity.Domain.Entities;
using Tracksys.Shared.Kernel.Results;

namespace Tracksys.Modules.Identity.Application.Services;

public class UserService(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
{
    public async Task<IReadOnlyList<UserDto>> GetAllAsync()
    {
        List<ApplicationUser> users = userManager.Users.ToList();
        var result = new List<UserDto>(users.Count);

        foreach (ApplicationUser user in users)
        {
            IList<string> roles = await userManager.GetRolesAsync(user);
            result.Add(new UserDto(user.Id, user.Email!, user.FullName, roles.ToList(), user.Scope, user.IsActive));
        }

        return result;
    }

    public async Task<Result<string>> CreateAsync(CreateUserRequest request)
    {
        if (!await roleManager.RoleExistsAsync(request.Role))
            return Result.Failure<string>($"Le rôle '{request.Role}' n'existe pas.");

        ApplicationUser user = new()
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            Scope = request.Scope,
            IsActive = true,
        };

        IdentityResult createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
            return Result.Failure<string>(string.Join(' ', createResult.Errors.Select(e => e.Description)));

        await userManager.AddToRoleAsync(user, request.Role);

        return Result.Success(user.Id);
    }

    public async Task<Result> ChangeRoleAsync(string userId, string role)
    {
        ApplicationUser? user = await userManager.FindByIdAsync(userId);
        if (user is null) return Result.Failure("Utilisateur introuvable.");

        if (!await roleManager.RoleExistsAsync(role))
            return Result.Failure($"Le rôle '{role}' n'existe pas.");

        IList<string> currentRoles = await userManager.GetRolesAsync(user);
        if (currentRoles.Count > 0) await userManager.RemoveFromRolesAsync(user, currentRoles);
        await userManager.AddToRoleAsync(user, role);

        return Result.Success();
    }

    public async Task<Result> SetActiveAsync(string userId, bool isActive)
    {
        ApplicationUser? user = await userManager.FindByIdAsync(userId);
        if (user is null) return Result.Failure("Utilisateur introuvable.");

        user.IsActive = isActive;
        await userManager.UpdateAsync(user);

        return Result.Success();
    }
}
