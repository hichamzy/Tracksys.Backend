namespace Tracksys.Modules.Fleet.Application.Dtos;

public record CreateVehicleRequest(
    string Code,
    string PlateNumber,
    int VehicleTypeId,
    string? Zone,
    string? ImeiTracker,
    string? FlespiIdent);
