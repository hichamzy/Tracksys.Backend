namespace Tracksys.Modules.Reports.Application.Dtos;

public record CreateSavedReportRequest(int ReportTypeId, string Name, string PeriodLabel, string Format);
