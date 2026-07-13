using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Tracksys.Shared.Kernel.Entities;
using Tracksys.Shared.Kernel.Repositories;

namespace Tracksys.Shared.Infrastructure.Persistence;

/// <summary>
/// Implémentation EF Core générique de <see cref="IRepository{TEntity,TId}"/>.
/// Un seul DbContext par module — voir <see cref="ModuleDbContext"/>.
/// </summary>
public class Repository<TEntity, TId>(DbContext dbContext) : IRepository<TEntity, TId>
    where TEntity : Entity<TId>
    where TId : notnull
{
    protected readonly DbContext DbContext = dbContext;
    protected readonly DbSet<TEntity> DbSet = dbContext.Set<TEntity>();

    public async Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default) =>
        await DbSet.FindAsync([id], cancellationToken);

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await DbSet.ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default) =>
        await DbSet.Where(predicate).ToListAsync(cancellationToken);

    public async Task<TEntity?> SingleOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default) =>
        await DbSet.SingleOrDefaultAsync(predicate, cancellationToken);

    public async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default) =>
        await DbSet.AnyAsync(predicate, cancellationToken);

    public IQueryable<TEntity> Query() => DbSet.AsQueryable();

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) =>
        await DbSet.AddAsync(entity, cancellationToken);

    public void Update(TEntity entity) => DbSet.Update(entity);

    public void Remove(TEntity entity) => DbSet.Remove(entity);
}
