using Tracksys.Shared.Kernel.Entities;

namespace Tracksys.Modules.Tenancy.Domain.Entities;

public class City : AuditableEntity<Guid>, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    private City() { }

    public static City Create(string name, string code)
    {
        var city = new City { Name = name, Code = code, IsActive = true };
        city.SetId(Guid.NewGuid());
        return city;
    }

    public void Rename(string name)
    {
        Name = name;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private void SetId(Guid id) => Id = id;
}
