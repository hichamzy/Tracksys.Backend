using Tracksys.Shared.Kernel.Entities;

namespace Tracksys.Modules.Fleet.Domain.Entities;

public class Circuit : Entity<int>
{
    public string Label { get; private set; } = string.Empty;
    public Guid CityId { get; private set; }

    private Circuit() { }

    public static Circuit Create(Guid cityId, string label) => new() { CityId = cityId, Label = label };
}
