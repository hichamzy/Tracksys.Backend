using System.Linq.Expressions;
using Tracksys.Shared.Kernel.Entities;

namespace Tracksys.Shared.Kernel.Repositories;

/// <summary>
/// Repository générique. Ne persiste rien tout seul : les écritures ne sont
/// effectives qu'après <see cref="Tracksys.Shared.Kernel.Persistence.IUnitOfWork.SaveChangesAsync"/>.
/// </summary>
public interface IRepository<TEntity, in TId>
    where TEntity : Entity<TId>
    where TId : notnull
{
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<TEntity?> SingleOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>IQueryable brut pour les cas nécessitant Include/pagination/tri côté Application.</summary>
    IQueryable<TEntity> Query();

    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    void Update(TEntity entity);

    void Remove(TEntity entity);
}
