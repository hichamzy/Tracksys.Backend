using Tracksys.Shared.Kernel.Entities;

namespace Tracksys.Modules.Reports.Domain.Entities;

public class FleetMonthlyStat : Entity<int>
{
    public string YearMonth { get; private set; } = string.Empty;
    public decimal TotalDistanceKm { get; private set; }
    public decimal ResolutionRatePct { get; private set; }
    public int ToursCompleted { get; private set; }
    public int ComplaintsHandled { get; private set; }
    public int AvgResponseMinutes { get; private set; }
    public Guid CityId { get; private set; }

    private FleetMonthlyStat() { }
}
