using Microsoft.EntityFrameworkCore.Storage;
using Tracksys.Modules.Identity.Application.Abstractions;
using Tracksys.Modules.Identity.Domain.Entities;
using Tracksys.Shared.Infrastructure.Persistence;
using Tracksys.Shared.Kernel.Repositories;

namespace Tracksys.Modules.Identity.Infrastructure.Persistence;

public class IdentityUnitOfWork(IdentityDbContext dbContext) : IIdentityUnitOfWork
{
    public IRepository<RefreshToken, long> RefreshTokens { get; } = new Repository<RefreshToken, long>(dbContext);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);

    public async Task<IDisposable> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        IDbContextTransaction tx = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        return tx;
    }
}
