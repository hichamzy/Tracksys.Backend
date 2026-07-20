using Tracksys.Modules.Tenancy.Application.Abstractions;
using Tracksys.Modules.Tenancy.Domain.Entities;
using Tracksys.Shared.Infrastructure.Persistence;
using Tracksys.Shared.Kernel.Repositories;

namespace Tracksys.Modules.Tenancy.Infrastructure.Persistence;

public class TenancyUnitOfWork : UnitOfWork<TenancyDbContext>, ITenancyUnitOfWork
{
    public IRepository<City, Guid> Cities { get; }

    public TenancyUnitOfWork(TenancyDbContext dbContext) : base(dbContext)
    {
        Cities = new Repository<City, Guid>(dbContext);
    }
}
