namespace Tracksys.Modules.Fleet.Application.Dtos;

public record VehicleDto(
    int Id,
    string Code,
    string PlateNumber,
    string Status,
    string? Zone,
    decimal SpeedKmh,
    decimal DistanceTodayKm,
    decimal? LastKnownLat,
    decimal? LastKnownLng,
    string? DriveTimeToday,
    string? LastStopLabel,
    string? DriverName,
    string? VehicleTypeLabel,
    string? ImeiTracker,
    string? FlespiIdent);
