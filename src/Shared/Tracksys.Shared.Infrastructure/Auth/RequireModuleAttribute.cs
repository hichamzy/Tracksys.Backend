using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Tracksys.Shared.Kernel.Auth;

namespace Tracksys.Shared.Infrastructure.Auth;

/// <summary>
/// Garde d'accès par module activé pour la ville — appliquer sur chaque controller
/// module-métier (ex. [RequireModule("fleet")] sur VehiclesController). Lit uniquement
/// les claims du JWT déjà validé (voir ICurrentTenantAccessor.EnabledModules), aucune
/// requête DB à chaque appel. Le SuperAdmin contourne toujours la vérification.
/// Fail-closed : un utilisateur non-SuperAdmin sans ce module dans ses claims => 403.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireModuleAttribute(string moduleCode) : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var tenant = context.HttpContext.RequestServices.GetRequiredService<ICurrentTenantAccessor>();

        if (!tenant.IsSuperAdmin && !tenant.EnabledModules.Contains(moduleCode))
        {
            context.Result = new ObjectResult(new { error = $"Module '{moduleCode}' non activé pour cette ville." })
            {
                StatusCode = StatusCodes.Status403Forbidden,
            };
            return;
        }

        await next();
    }
}
