using Tracksys.Modules.Fleet.Application.Abstractions;
using Tracksys.Modules.Fleet.Application.Dtos;

namespace Tracksys.Modules.Fleet.Application.Services;

public class DriverQueryService(IFleetUnitOfWork unitOfWork)
{
    public async Task<IReadOnlyList<DriverDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var drivers = await unitOfWork.Drivers.GetAllAsync(cancellationToken);
        var vehicles = await unitOfWork.Vehicles.GetAllAsync(cancellationToken);
        var vehiclesById = vehicles.ToDictionary(v => v.Id);

        return drivers
            .Select(d => new DriverDto(
                d.Id, d.FullName, d.Phone, d.LicenceNumber, d.LicenceValid, d.Status,
                d.CurrentVehicleId,
                d.CurrentVehicleId.HasValue && vehiclesById.TryGetValue(d.CurrentVehicleId.Value, out var vehicle) ? vehicle.Code : null))
            .ToList();
    }
}
