using Tracksys.Shared.Kernel.Entities;
using Tracksys.Shared.Kernel.Guards;

namespace Tracksys.Modules.Fleet.Domain.Entities;

/// <summary>Famille SinoTrack (device.type.id 870) — engin distinct de Vehicle (Teltonika), opéré par un délégataire.</summary>
public class Chariot : AuditableEntity<int>, IAggregateRoot
{
    public string Numero { get; private set; } = string.Empty;
    public int? DelegataireId { get; private set; }
    public int? BoitierId { get; private set; }

    /// <summary>Identifiant déclaré par le tracker à Flespi (`ident`) — PAS nécessairement l'IMEI (ex. SinoTrack : ident="014543268085" ≠ IMEI réel décodé "868791088059303").</summary>
    public string? FlespiIdent { get; private set; }

    public decimal? LastKnownLat { get; private set; }
    public decimal? LastKnownLng { get; private set; }
    public DateTime? LastPositionAtUtc { get; private set; }

    private Chariot() { }

    public static Chariot Create(string numero, int? delegataireId, int? boitierId, string? flespiIdent = null) => new()
    {
        Numero = numero,
        DelegataireId = delegataireId,
        BoitierId = boitierId,
        FlespiIdent = flespiIdent,
    };

    public void SetFlespiIdent(string flespiIdent) => FlespiIdent = flespiIdent;

    public void UpdateLastKnownPosition(decimal lat, decimal lng, DateTime observedAtUtc)
    {
        LastKnownLat = lat;
        LastKnownLng = lng;
        LastPositionAtUtc = DateTimeGuard.EnsureUtc(observedAtUtc, nameof(observedAtUtc));
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
