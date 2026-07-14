using Tracksys.Shared.Kernel.Entities;
using Tracksys.Shared.Kernel.Guards;

namespace Tracksys.Modules.Ingestion.Domain.Entities;

public static class IngestAnomalyReasons
{
    public const string PositionInvalide = "position_invalide";
    public const string TimestampAberrant = "timestamp_aberrant";
    public const string DeviceInconnu = "device_inconnu";
    public const string PlanningIntrouvable = "planning_introuvable";
    public const string IdentAmbigu = "ident_ambigu";
}

public class IngestAnomaly : Entity<long>, IAggregateRoot
{
    public DateTime TimeUtc { get; private set; }
    public string? Ident { get; private set; }
    public string Raison { get; private set; } = string.Empty;
    public string PayloadBrut { get; private set; } = string.Empty;

    private IngestAnomaly() { }

    public static IngestAnomaly Create(string? ident, string raison, string payloadBrutJson, DateTime timeUtc) => new()
    {
        Ident = ident,
        Raison = raison,
        PayloadBrut = payloadBrutJson,
        TimeUtc = DateTimeGuard.EnsureUtc(timeUtc, nameof(timeUtc)),
    };
}
