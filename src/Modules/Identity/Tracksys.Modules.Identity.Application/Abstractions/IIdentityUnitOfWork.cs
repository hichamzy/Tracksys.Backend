using Tracksys.Modules.Identity.Domain.Entities;
using Tracksys.Shared.Kernel.Persistence;
using Tracksys.Shared.Kernel.Repositories;

namespace Tracksys.Modules.Identity.Application.Abstractions;

public interface IIdentityUnitOfWork : IUnitOfWork
{
    IRepository<RefreshToken, long> RefreshTokens { get; }

    /// <summary>
    /// Recherche un utilisateur sans appliquer le HasQueryFilter par ville — nécessaire pour le
    /// login et le refresh de token, où aucun contexte tenant (JWT) n'existe encore : le filtre
    /// exclurait sinon tout utilisateur rattaché à une ville, empêchant quiconque de se connecter.
    /// </summary>
    Task<ApplicationUser?> FindUserByEmailIgnoringTenantAsync(string normalizedEmail, CancellationToken cancellationToken = default);

    Task<ApplicationUser?> FindUserByIdIgnoringTenantAsync(string userId, CancellationToken cancellationToken = default);
}
