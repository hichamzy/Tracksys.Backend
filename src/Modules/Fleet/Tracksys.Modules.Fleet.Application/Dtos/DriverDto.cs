namespace Tracksys.Modules.Fleet.Application.Dtos;

public record DriverDto(
    int Id,
    string FullName,
    string? Phone,
    string? LicenceNumber,
    bool LicenceValid,
    string Status,
    int? CurrentVehicleId,
    string? CurrentVehicleCode);
