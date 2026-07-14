namespace Tracksys.Modules.Ingestion.Domain.Entities;

/// <summary>
/// Point de télémétrie mappé et validé, prêt à écrire dans ingestion.telemetry /
/// ingestion.last_position. Toutes les colonnes enrichies (VehicleId/ChariotId/...)
/// sont déjà résolues par jointure au moment de la construction — jamais recalculées
/// après coup (planning_id figé pour l'historique, voir CLAUDE.md).
/// </summary>
public sealed class TelemetryPoint
{
    public required string Ident { get; init; }
    public required DateTime DeviceTsUtc { get; init; }
    public required DateTime ServerTsUtc { get; init; }
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
    public float? PositionSpeed { get; init; }
    public float? BatteryLevel { get; init; }
    public float? BatteryVoltage { get; init; }

    /// <summary>Toujours NULL pour l'instant — aucune règle de dérivation fiable identifiée dans les payloads Flespi observés.</summary>
    public bool? IsPowerbankConnected { get; init; }

    public int? ChariotId { get; init; }
    public string? ChariotNumero { get; init; }
    public int? BoitierId { get; init; }
    public int? DelegataireId { get; init; }
    public long? PlanningId { get; init; }
    public int? CircuitId { get; init; }
    public int? TypePrestationId { get; init; }
}
