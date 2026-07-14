using Tracksys.Shared.Kernel.Entities;

namespace Tracksys.Modules.Fleet.Domain.Entities;

public class Delegataire : Entity<int>
{
    public string Label { get; private set; } = string.Empty;

    private Delegataire() { }

    public static Delegataire Create(string label) => new() { Label = label };
}
