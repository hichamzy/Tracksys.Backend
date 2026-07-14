using Tracksys.Modules.Fleet.Application.Abstractions;
using Tracksys.Modules.Fleet.Application.Dtos;

namespace Tracksys.Modules.Fleet.Application.Services;

public class VehicleQueryService(IFleetUnitOfWork unitOfWork)
{
    public async Task<IReadOnlyList<VehicleDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var vehicles = await unitOfWork.Vehicles.GetAllAsync(cancellationToken);
        var drivers = await unitOfWork.Drivers.GetAllAsync(cancellationToken);
        var vehicleTypes = await unitOfWork.VehicleTypes.GetAllAsync(cancellationToken);

        var driversById = drivers.ToDictionary(d => d.Id);
        var vehicleTypesById = vehicleTypes.ToDictionary(t => t.Id);

        return vehicles
            .Select(v => new VehicleDto(
                v.Id, v.Code, v.PlateNumber, v.Status.ToString(), v.Zone,
                v.SpeedKmh, v.DistanceTodayKm, v.LastKnownLat, v.LastKnownLng,
                v.DriveTimeToday, v.LastStopLabel,
                v.DriverId.HasValue && driversById.TryGetValue(v.DriverId.Value, out var driver) ? driver.FullName : null,
                vehicleTypesById.TryGetValue(v.VehicleTypeId, out var vehicleType) ? vehicleType.Label : null,
                v.ImeiTracker, v.FlespiIdent))
            .ToList();
    }
}
