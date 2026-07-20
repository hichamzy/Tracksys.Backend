using Tracksys.Modules.Tenancy.Domain.Entities;
using Tracksys.Shared.Kernel.Persistence;
using Tracksys.Shared.Kernel.Repositories;

namespace Tracksys.Modules.Tenancy.Application.Abstractions;

public interface ITenancyUnitOfWork : IUnitOfWork
{
    IRepository<City, Guid> Cities { get; }
    IRepository<CityModule, Guid> CityModules { get; }
}
