using Tracksys.Modules.Fleet.Application.Abstractions;
using Tracksys.Modules.Fleet.Application.Dtos;

namespace Tracksys.Modules.Fleet.Application.Services;

public class VehicleTypeQueryService(IFleetUnitOfWork unitOfWork)
{
    public async Task<IReadOnlyList<VehicleTypeDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var vehicleTypes = await unitOfWork.VehicleTypes.GetAllAsync(cancellationToken);
        return vehicleTypes.Select(t => new VehicleTypeDto(t.Id, t.Label)).ToList();
    }
}
