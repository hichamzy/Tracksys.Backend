using Tracksys.Shared.Kernel.Entities;

namespace Tracksys.Modules.Tenancy.Domain.Entities;

/// <summary>
/// Module activé pour une ville (droits d'accès) — identifiants alignés sur les clés
/// de vue front (dash, fleet, hist, alerts, report, cit, settings). Un module non
/// activé rend le controller backend associé inaccessible (voir RequireModuleAttribute)
/// et masque l'item correspondant côté sidebar.
/// </summary>
public class CityModule : Entity<Guid>
{
    public Guid CityId { get; private set; }
    public string ModuleCode { get; private set; } = string.Empty;

    private CityModule() { }

    public static CityModule Create(Guid cityId, string moduleCode)
    {
        var module = new CityModule { CityId = cityId, ModuleCode = moduleCode };
        module.SetId(Guid.NewGuid());
        return module;
    }

    private void SetId(Guid id) => Id = id;
}
