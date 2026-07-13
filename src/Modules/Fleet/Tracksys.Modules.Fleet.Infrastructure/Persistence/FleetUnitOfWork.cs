using Tracksys.Modules.Fleet.Application.Abstractions;
using Tracksys.Modules.Fleet.Domain.Entities;
using Tracksys.Shared.Infrastructure.Persistence;
using Tracksys.Shared.Kernel.Repositories;

namespace Tracksys.Modules.Fleet.Infrastructure.Persistence;

public class FleetUnitOfWork : UnitOfWork<FleetDbContext>, IFleetUnitOfWork
{
    public IRepository<Vehicle, int> Vehicles { get; }
    public IRepository<Driver, int> Drivers { get; }
    public IRepository<VehicleType, int> VehicleTypes { get; }

    public FleetUnitOfWork(FleetDbContext dbContext) : base(dbContext)
    {
        Vehicles = new Repository<Vehicle, int>(dbContext);
        Drivers = new Repository<Driver, int>(dbContext);
        VehicleTypes = new Repository<VehicleType, int>(dbContext);
    }
}
