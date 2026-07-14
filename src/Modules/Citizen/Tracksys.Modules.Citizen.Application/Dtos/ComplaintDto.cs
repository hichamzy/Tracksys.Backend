namespace Tracksys.Modules.Citizen.Application.Dtos;

public record ComplaintDto(
    int Id,
    string Code,
    int CategoryId,
    string CategoryLabel,
    string Priority,
    string Status,
    string ZoneLabel,
    decimal Lat,
    decimal Lng,
    int? AssignedVehicleId,
    string? ReporterName,
    DateTime ReportedAtUtc,
    DateTime? ResolvedAtUtc,
    string? PhotoBeforeUrl,
    string? PhotoAfterUrl);
