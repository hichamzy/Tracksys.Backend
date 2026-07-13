using Microsoft.EntityFrameworkCore.Storage;
using Tracksys.Shared.Kernel.Persistence;

namespace Tracksys.Shared.Infrastructure.Persistence;

/// <summary>Implémentation générique de <see cref="IUnitOfWork"/> pour un DbContext de module donné.</summary>
public class UnitOfWork<TContext>(TContext dbContext) : IUnitOfWork
    where TContext : ModuleDbContext
{
    protected readonly TContext DbContext = dbContext;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        DbContext.SaveChangesAsync(cancellationToken);

    public async Task<IDisposable> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        IDbContextTransaction tx = await DbContext.Database.BeginTransactionAsync(cancellationToken);
        return tx;
    }
}
