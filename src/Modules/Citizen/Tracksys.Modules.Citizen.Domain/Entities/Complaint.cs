using Tracksys.Modules.Citizen.Domain.Enums;
using Tracksys.Shared.Kernel.Entities;

namespace Tracksys.Modules.Citizen.Domain.Entities;

public class Complaint : AuditableEntity<int>, IAggregateRoot
{
    public string Code { get; private set; } = string.Empty;
    public int CategoryId { get; private set; }
    public string Priority { get; private set; } = "Moyenne";
    public ComplaintStatus Status { get; private set; } = ComplaintStatus.Received;
    public string ZoneLabel { get; private set; } = string.Empty;
    public decimal Lat { get; private set; }
    public decimal Lng { get; private set; }
    public int? AssignedVehicleId { get; private set; }
    public string? ReporterName { get; private set; }
    public DateTime ReportedAtUtc { get; private set; } = DateTime.UtcNow;
    public DateTime? ResolvedAtUtc { get; private set; }
    public string? PhotoBeforeUrl { get; private set; }
    public string? PhotoAfterUrl { get; private set; }

    private Complaint() { }

    public static Complaint Create(
        string code, int categoryId, string priority, string zoneLabel, decimal lat, decimal lng, string? reporterName) => new()
    {
        Code = code,
        CategoryId = categoryId,
        Priority = priority,
        ZoneLabel = zoneLabel,
        Lat = lat,
        Lng = lng,
        ReporterName = reporterName ?? "Anonyme",
        Status = ComplaintStatus.Received,
    };

    public void AssignVehicle(int vehicleId)
    {
        AssignedVehicleId = vehicleId;
        Status = ComplaintStatus.InProgress;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Resolve(string? photoAfterUrl)
    {
        Status = ComplaintStatus.Resolved;
        ResolvedAtUtc = DateTime.UtcNow;
        PhotoAfterUrl = photoAfterUrl;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
