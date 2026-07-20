namespace Tracksys.Shared.Kernel.Auth;

/// <summary>
/// Résout les modules activés pour une ville — abstraction cross-module (implémentée par
/// Tenancy.Infrastructure) consommée par AuthService (Identity.Application) pour encoder
/// le claim JWT "module" au login/refresh, sans créer de dépendance directe entre les
/// modules métier Identity et Tenancy (chacun ne référence que Shared.Kernel).
/// </summary>
public interface ICityModuleResolver
{
    Task<IReadOnlyList<string>> GetEnabledModuleCodesAsync(Guid cityId, CancellationToken cancellationToken = default);
}
