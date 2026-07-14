using Tracksys.Shared.Kernel.Entities;

namespace Tracksys.Modules.Reports.Domain.Entities;

public class SavedReport : Entity<int>, IAggregateRoot
{
    public int ReportTypeId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string PeriodLabel { get; private set; } = string.Empty;
    public string Format { get; private set; } = "XLSX";
    public string? FileUrl { get; private set; }
    public DateTime GeneratedAtUtc { get; private set; } = DateTime.UtcNow;
    public string? GeneratedByUserId { get; private set; }

    private SavedReport() { }

    public static SavedReport Create(int reportTypeId, string name, string periodLabel, string format, string? generatedByUserId) => new()
    {
        ReportTypeId = reportTypeId,
        Name = name,
        PeriodLabel = periodLabel,
        Format = format,
        GeneratedByUserId = generatedByUserId,
        GeneratedAtUtc = DateTime.UtcNow,
    };
}
