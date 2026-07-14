namespace Tracksys.Modules.Ingestion.Application.Abstractions;

public record LookupResult(
    bool VehicleMatched,
    bool ChariotMatched,
    int? ChariotId,
    string? ChariotNumero,
    int? BoitierId,
    int? DelegataireId,
    long? PlanningId,
    int? CircuitId,
    int? TypePrestationId);

/// <summary>
/// Résout les colonnes enrichies par jointure directe sur fleet.vehicles/fleet.chariots/
/// fleet.plannings (base unique — pas de table ref_* dupliquée, voir CLAUDE.md).
/// </summary>
public interface ILookupService
{
    Task<LookupResult> ResolveAsync(string ident, DateTime deviceTsUtc, CancellationToken cancellationToken = default);
}
