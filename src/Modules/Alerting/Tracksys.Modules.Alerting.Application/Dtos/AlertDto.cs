namespace Tracksys.Modules.Alerting.Application.Dtos;

public record AlertDto(
    long Id,
    string Code,
    string AlertTypeCode,
    string AlertTypeLabel,
    string Severity,
    int VehicleId,
    string DetailText,
    DateTime OccurredAtUtc,
    bool IsUnread,
    DateTime? ReadAtUtc,
    string? ReadByUserId);
