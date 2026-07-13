using Tracksys.Shared.Kernel.Entities;

namespace Tracksys.Modules.Fleet.Domain.Entities;

public class VehicleType : Entity<int>
{
    public string Label { get; private set; } = string.Empty;

    private VehicleType() { }

    public static VehicleType Create(string label) => new() { Label = label };
}
