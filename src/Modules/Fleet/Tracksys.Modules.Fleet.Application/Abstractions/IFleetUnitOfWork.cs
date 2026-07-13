using Tracksys.Modules.Fleet.Domain.Entities;
using Tracksys.Shared.Kernel.Persistence;
using Tracksys.Shared.Kernel.Repositories;

namespace Tracksys.Modules.Fleet.Application.Abstractions;

public interface IFleetUnitOfWork : IUnitOfWork
{
    IRepository<Vehicle, int> Vehicles { get; }
    IRepository<Driver, int> Drivers { get; }
    IRepository<VehicleType, int> VehicleTypes { get; }
}
