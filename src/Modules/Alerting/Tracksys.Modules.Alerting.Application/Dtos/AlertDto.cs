namespace Tracksys.Modules.Alerting.Application.Dtos;

public record AlertDto(
    long Id,
    string Code,
    string AlertTypeCode,
    int VehicleId,
    string DetailText,
    DateTime OccurredAtUtc,
    bool IsUnread);
