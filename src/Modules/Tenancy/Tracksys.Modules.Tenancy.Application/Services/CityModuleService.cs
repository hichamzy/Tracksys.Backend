using Tracksys.Modules.Tenancy.Application.Abstractions;
using Tracksys.Modules.Tenancy.Application.Dtos;
using Tracksys.Modules.Tenancy.Domain.Entities;

namespace Tracksys.Modules.Tenancy.Application.Services;

public class CityModuleService(ITenancyUnitOfWork unitOfWork)
{
    /// <summary>Identifiants de vue front (dash, fleet, hist, alerts, report, cit, settings) — une ville active tout par défaut à la création.</summary>
    public static readonly IReadOnlyList<string> AllModuleCodes =
        ["dash", "fleet", "hist", "alerts", "report", "cit", "settings"];

    public async Task<CityModulesDto> GetEnabledModulesAsync(Guid cityId, CancellationToken cancellationToken = default)
    {
        var modules = await unitOfWork.CityModules.FindAsync(m => m.CityId == cityId, cancellationToken);
        return new CityModulesDto(cityId, modules.Select(m => m.ModuleCode).ToList());
    }

    public async Task SeedDefaultModulesAsync(Guid cityId, CancellationToken cancellationToken = default)
    {
        foreach (string code in AllModuleCodes)
            await unitOfWork.CityModules.AddAsync(CityModule.Create(cityId, code), cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task SetEnabledModulesAsync(Guid cityId, IReadOnlyList<string> moduleCodes, CancellationToken cancellationToken = default)
    {
        var existing = await unitOfWork.CityModules.FindAsync(m => m.CityId == cityId, cancellationToken);
        foreach (CityModule module in existing)
            unitOfWork.CityModules.Remove(module);

        foreach (string code in moduleCodes.Distinct())
            await unitOfWork.CityModules.AddAsync(CityModule.Create(cityId, code), cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
