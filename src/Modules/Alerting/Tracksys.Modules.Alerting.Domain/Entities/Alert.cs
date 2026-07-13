using Tracksys.Shared.Kernel.Entities;

namespace Tracksys.Modules.Alerting.Domain.Entities;

public class Alert : Entity<long>, IAggregateRoot
{
    public string Code { get; private set; } = string.Empty;
    public string AlertTypeCode { get; private set; } = string.Empty;
    public int VehicleId { get; private set; }
    public string DetailText { get; private set; } = string.Empty;
    public DateTime OccurredAtUtc { get; private set; } = DateTime.UtcNow;
    public bool IsUnread { get; private set; } = true;
    public DateTime? ReadAtUtc { get; private set; }
    public string? ReadByUserId { get; private set; }

    private Alert() { }

    public static Alert Create(string code, string alertTypeCode, int vehicleId, string detailText) => new()
    {
        Code = code,
        AlertTypeCode = alertTypeCode,
        VehicleId = vehicleId,
        DetailText = detailText,
        IsUnread = true,
    };

    public void MarkAsRead(string userId)
    {
        IsUnread = false;
        ReadAtUtc = DateTime.UtcNow;
        ReadByUserId = userId;
    }
}
