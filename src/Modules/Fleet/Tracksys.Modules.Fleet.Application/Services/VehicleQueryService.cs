using Tracksys.Modules.Fleet.Application.Abstractions;
using Tracksys.Modules.Fleet.Application.Dtos;

namespace Tracksys.Modules.Fleet.Application.Services;

public class VehicleQueryService(IFleetUnitOfWork unitOfWork)
{
    public async Task<IReadOnlyList<VehicleDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var vehicles = await unitOfWork.Vehicles.GetAllAsync(cancellationToken);

        return vehicles
            .Select(v => new VehicleDto(
                v.Id, v.Code, v.PlateNumber, v.Status.ToString(), v.Zone,
                v.SpeedKmh, v.DistanceTodayKm, v.LastKnownLat, v.LastKnownLng))
            .ToList();
    }
}
