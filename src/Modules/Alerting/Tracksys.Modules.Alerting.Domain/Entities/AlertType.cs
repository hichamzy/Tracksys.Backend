using Tracksys.Shared.Kernel.Entities;

namespace Tracksys.Modules.Alerting.Domain.Entities;

public class AlertType : Entity<string>
{
    public string Label { get; private set; } = string.Empty;
    public string Severity { get; private set; } = "md"; // 'hi' | 'md' | 'lo'

    private AlertType() { }

    public static AlertType Create(string code, string label, string severity)
    {
        var type = new AlertType { Label = label, Severity = severity };
        type.SetId(code);
        return type;
    }

    private void SetId(string code) => Id = code;
}
