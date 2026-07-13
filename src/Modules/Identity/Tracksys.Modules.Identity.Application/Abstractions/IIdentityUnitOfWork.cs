using Tracksys.Modules.Identity.Domain.Entities;
using Tracksys.Shared.Kernel.Persistence;
using Tracksys.Shared.Kernel.Repositories;

namespace Tracksys.Modules.Identity.Application.Abstractions;

public interface IIdentityUnitOfWork : IUnitOfWork
{
    IRepository<RefreshToken, long> RefreshTokens { get; }
}
