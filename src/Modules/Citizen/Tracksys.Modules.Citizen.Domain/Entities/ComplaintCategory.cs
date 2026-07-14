using Tracksys.Shared.Kernel.Entities;

namespace Tracksys.Modules.Citizen.Domain.Entities;

public class ComplaintCategory : Entity<int>
{
    public string Label { get; private set; } = string.Empty;
    public string? Icon { get; private set; }
    public string DefaultPriority { get; private set; } = "Moyenne";
    public int SlaHours { get; private set; }
    public bool IsActive { get; private set; } = true;

    private ComplaintCategory() { }

    public static ComplaintCategory Create(string label, string? icon, string defaultPriority, int slaHours) => new()
    {
        Label = label,
        Icon = icon,
        DefaultPriority = defaultPriority,
        SlaHours = slaHours,
    };

    public void UpdateDetails(string label, string? icon, string defaultPriority, int slaHours)
    {
        Label = label;
        Icon = icon;
        DefaultPriority = defaultPriority;
        SlaHours = slaHours;
    }

    public void SetActive(bool isActive) => IsActive = isActive;
}
