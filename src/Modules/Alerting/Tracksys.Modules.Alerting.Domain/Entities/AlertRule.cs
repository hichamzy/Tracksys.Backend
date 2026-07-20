using Tracksys.Shared.Kernel.Entities;

namespace Tracksys.Modules.Alerting.Domain.Entities;

public class AlertRule : Entity<int>, IAggregateRoot
{
    public string AlertTypeCode { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; } = true;
    public decimal Threshold { get; private set; }
    public string Unit { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public Guid CityId { get; private set; }

    private AlertRule() { }

    public static AlertRule Create(Guid cityId, string alertTypeCode, decimal threshold, string unit, string? description) => new()
    {
        CityId = cityId,
        AlertTypeCode = alertTypeCode,
        Threshold = threshold,
        Unit = unit,
        Description = description,
        IsEnabled = true,
    };

    public void UpdateThreshold(decimal threshold)
    {
        Threshold = threshold;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Toggle(bool enabled)
    {
        IsEnabled = enabled;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
