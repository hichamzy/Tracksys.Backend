using Microsoft.AspNetCore.Http;
using Tracksys.Shared.Kernel.Auth;

namespace Tracksys.Shared.Infrastructure.Auth;

public class HttpContextTenantAccessor(IHttpContextAccessor accessor) : ICurrentTenantAccessor
{
    public Guid? CityId
    {
        get
        {
            string? raw = accessor.HttpContext?.User.FindFirst("city_id")?.Value;
            return Guid.TryParse(raw, out Guid id) ? id : null;
        }
    }

    public bool IsSuperAdmin => accessor.HttpContext?.User.IsInRole("SuperAdmin") ?? false;

    public IReadOnlyCollection<string> EnabledModules =>
        accessor.HttpContext?.User.FindAll("module").Select(c => c.Value).ToArray() ?? [];
}
