using Tracksys.Shared.Kernel.Entities;

namespace Tracksys.Modules.Fleet.Domain.Entities;

public class TypePrestation : Entity<int>
{
    public string Label { get; private set; } = string.Empty;

    private TypePrestation() { }

    public static TypePrestation Create(string label) => new() { Label = label };
}
