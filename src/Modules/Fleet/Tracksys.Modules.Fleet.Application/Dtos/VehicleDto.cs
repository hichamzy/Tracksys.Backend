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
    decimal? LastKnownLng);
