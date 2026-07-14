namespace Tracksys.Modules.Alerting.Application.Dtos;

public record AlertRuleDto(
    int Id,
    string AlertTypeCode,
    string AlertTypeLabel,
    bool IsEnabled,
    decimal Threshold,
    string Unit,
    string? Description);
