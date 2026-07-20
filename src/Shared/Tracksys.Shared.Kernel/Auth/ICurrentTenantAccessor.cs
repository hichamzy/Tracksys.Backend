namespace Tracksys.Shared.Kernel.Auth;

/// <summary>
/// Résout la ville (tenant) de la requête courante depuis le JWT — utilisé par le
/// HasQueryFilter global de chaque DbContext de module pour isoler les données par ville,
/// et par les services Application pour dériver le CityId à la création d'une entité
/// (jamais accepté tel quel depuis un payload client).
/// CityId null + IsSuperAdmin false (token absent/invalide/sans claim city_id) => le filtre
/// ne matche aucune ligne (fail-closed), jamais traité comme un accès total.
/// </summary>
public interface ICurrentTenantAccessor
{
    Guid? CityId { get; }
    bool IsSuperAdmin { get; }

    /// <summary>Codes de modules activés lus depuis les claims "module" du JWT courant.
    /// Vide pour un SuperAdmin (jamais émis, non pertinent — accès total implicite) ou
    /// pour un token sans claim (fail-closed côté RequireModuleAttribute).</summary>
    IReadOnlyCollection<string> EnabledModules { get; }
}
