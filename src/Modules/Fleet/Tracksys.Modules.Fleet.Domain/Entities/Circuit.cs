using Tracksys.Shared.Kernel.Entities;

namespace Tracksys.Modules.Fleet.Domain.Entities;

public class Circuit : Entity<int>
{
    public string Label { get; private set; } = string.Empty;

    private Circuit() { }

    public static Circuit Create(string label) => new() { Label = label };
}
