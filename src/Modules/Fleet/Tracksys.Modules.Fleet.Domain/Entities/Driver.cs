using Tracksys.Shared.Kernel.Entities;

namespace Tracksys.Modules.Fleet.Domain.Entities;

public class Driver : AuditableEntity<int>, IAggregateRoot
{
    public string FullName { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public string? LicenceNumber { get; private set; }
    public bool LicenceValid { get; private set; } = true;
    public string Status { get; private set; } = "En service";
    public string? ApplicationUserId { get; private set; }
    public int? CurrentVehicleId { get; private set; }

    private Driver() { }

    public static Driver Create(string fullName, string? phone, string? licenceNumber) => new()
    {
        FullName = fullName,
        Phone = phone,
        LicenceNumber = licenceNumber,
    };

    public void AssignToVehicle(int vehicleId) => CurrentVehicleId = vehicleId;

    public void Unassign() => CurrentVehicleId = null;

    public void ChangeStatus(string status)
    {
        Status = status;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
