using Microsoft.EntityFrameworkCore;
using Tracksys.Modules.Tenancy.Infrastructure.Persistence;
using Tracksys.Shared.Kernel.Auth;

namespace Tracksys.Modules.Tenancy.Infrastructure.Auth;

public class CityModuleResolver(TenancyDbContext dbContext) : ICityModuleResolver
{
    public async Task<IReadOnlyList<string>> GetEnabledModuleCodesAsync(Guid cityId, CancellationToken cancellationToken = default) =>
        await dbContext.CityModules
            .Where(m => m.CityId == cityId)
            .Select(m => m.ModuleCode)
            .ToListAsync(cancellationToken);
}
