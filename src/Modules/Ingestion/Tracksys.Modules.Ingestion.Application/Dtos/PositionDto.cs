namespace Tracksys.Modules.Ingestion.Application.Dtos;

public record PositionDto(
    string Ident,
    DateTime DeviceTsUtc,
    double Latitude,
    double Longitude,
    float? PositionSpeed,
    float? BatteryLevel,
    int? ChariotId,
    string? ChariotNumero,
    int? DelegataireId,
    long? PlanningId,
    int? CircuitId);
