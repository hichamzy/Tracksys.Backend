using Tracksys.Shared.Kernel.Entities;

namespace Tracksys.Modules.Reports.Domain.Entities;

public class ReportType : Entity<int>
{
    public string Label { get; private set; } = string.Empty;

    private ReportType() { }

    public static ReportType Create(string label) => new() { Label = label };
}
