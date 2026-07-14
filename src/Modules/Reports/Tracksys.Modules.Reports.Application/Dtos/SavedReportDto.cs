namespace Tracksys.Modules.Reports.Application.Dtos;

public record SavedReportDto(
    int Id,
    string ReportTypeLabel,
    string Name,
    string PeriodLabel,
    string Format,
    string? FileUrl,
    DateTime GeneratedAtUtc);
