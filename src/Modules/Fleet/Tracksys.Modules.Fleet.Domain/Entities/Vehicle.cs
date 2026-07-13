using Tracksys.Modules.Fleet.Domain.Enums;
using Tracksys.Shared.Kernel.Entities;

namespace Tracksys.Modules.Fleet.Domain.Entities;

public class Vehicle : AuditableEntity<int>, IAggregateRoot
{
    public string Code { get; private set; } = string.Empty;
    public string PlateNumber { get; private set; } = string.Empty;
    public int VehicleTypeId { get; private set; }
    public int? DriverId { get; private set; }
    public VehicleStatus Status { get; private set; } = VehicleStatus.Idle;
    public string? Zone { get; private set; }
    public string? ImeiTracker { get; private set; }
    public decimal SpeedKmh { get; private set; }
    public decimal DistanceTodayKm { get; private set; }
    public string? DriveTimeToday { get; private set; }
    public string? LastStopLabel { get; private set; }
    public decimal? LastKnownLat { get; private set; }
    public decimal? LastKnownLng { get; private set; }
    public DateTime? LastPositionAtUtc { get; private set; }

    private Vehicle() { }

    public static Vehicle Create(string code, string plateNumber, int vehicleTypeId, string? zone, string? imeiTracker) => new()
    {
        Code = code,
        PlateNumber = plateNumber,
        VehicleTypeId = vehicleTypeId,
        Zone = zone,
        ImeiTracker = imeiTracker,
        Status = VehicleStatus.Idle,
    };

    public void AssignDriver(int driverId) => DriverId = driverId;

    public void UnassignDriver() => DriverId = null;

    public void ChangeStatus(VehicleStatus status)
    {
        Status = status;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>Met à jour le cache de position — source de vérité : le service qui lit PostgreSQL/PostGIS.</summary>
    public void UpdateLastKnownPosition(decimal lat, decimal lng, decimal speedKmh, DateTime observedAtUtc)
    {
        LastKnownLat = lat;
        LastKnownLng = lng;
        SpeedKmh = speedKmh;
        LastPositionAtUtc = observedAtUtc;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
