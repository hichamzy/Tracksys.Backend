using Tracksys.Modules.Ingestion.Application.Abstractions;
using Tracksys.Modules.Ingestion.Application.Dtos;
using Tracksys.Modules.Ingestion.Domain.Entities;

namespace Tracksys.Modules.Ingestion.Application.Mapping;

public record MappingOutcome(TelemetryPoint? Point, IngestAnomaly? Anomaly);

/// <summary>
/// Mappe un message Flespi brut vers un TelemetryPoint validé, ou une IngestAnomaly
/// si le point ne peut pas/ne doit pas être inséré. Un message peut produire les DEUX
/// (point conservé + anomalie loggée) — ex. device inconnu ou planning introuvable :
/// on n'écarte jamais un point GPS valide, on enrichit juste moins.
/// </summary>
public class FlespiMapper(ILookupService lookupService, TimeProvider timeProvider)
{
    private static readonly TimeSpan MaxTimestampDrift = TimeSpan.FromHours(24);

    public async Task<MappingOutcome> MapAsync(FlespiMessageDto dto, string rawJson, CancellationToken cancellationToken = default)
    {
        DateTime serverTsUtc = ParseServerTimestamp(dto, out bool serverTsMissing);
        DateTime nowUtc = timeProvider.GetUtcNow().UtcDateTime;

        if (string.IsNullOrWhiteSpace(dto.Ident))
            return AnomalyOnly(null, IngestAnomalyReasons.DeviceInconnu, rawJson, nowUtc);

        if (!IsPositionValid(dto))
            return AnomalyOnly(dto.Ident, IngestAnomalyReasons.PositionInvalide, rawJson, nowUtc);

        (DateTime deviceTsUtc, bool timestampAberrant) = ResolveDeviceTimestamp(dto, serverTsUtc, serverTsMissing);

        LookupResult lookup = await lookupService.ResolveAsync(dto.Ident, deviceTsUtc, cancellationToken);

        if (lookup.VehicleMatched && lookup.ChariotMatched)
            return AnomalyOnly(dto.Ident, IngestAnomalyReasons.IdentAmbigu, rawJson, nowUtc);

        var point = new TelemetryPoint
        {
            Ident = dto.Ident,
            DeviceTsUtc = deviceTsUtc,
            ServerTsUtc = serverTsUtc,
            Latitude = dto.PositionLatitude!.Value,
            Longitude = dto.PositionLongitude!.Value,
            PositionSpeed = dto.PositionSpeed,
            BatteryLevel = dto.BatteryLevel,
            BatteryVoltage = dto.BatteryVoltage,
            IsPowerbankConnected = null, // aucune règle de dérivation fiable identifiée — voir CLAUDE.md
            ChariotId = lookup.ChariotId,
            ChariotNumero = lookup.ChariotNumero,
            BoitierId = lookup.BoitierId,
            DelegataireId = lookup.DelegataireId,
            PlanningId = lookup.PlanningId,
            CircuitId = lookup.CircuitId,
            TypePrestationId = lookup.TypePrestationId,
        };

        IngestAnomaly? anomaly = null;
        if (timestampAberrant)
            anomaly = IngestAnomaly.Create(dto.Ident, IngestAnomalyReasons.TimestampAberrant, rawJson, nowUtc);
        else if (!lookup.VehicleMatched && !lookup.ChariotMatched)
            anomaly = IngestAnomaly.Create(dto.Ident, IngestAnomalyReasons.DeviceInconnu, rawJson, nowUtc);
        else if (lookup.ChariotMatched && lookup.PlanningId is null)
            anomaly = IngestAnomaly.Create(dto.Ident, IngestAnomalyReasons.PlanningIntrouvable, rawJson, nowUtc);

        return new MappingOutcome(point, anomaly);
    }

    /// <summary>
    /// position.valid=false -> invalide. Champ absent (famille Teltonika, qui ne
    /// l'envoie pas) -> on retombe sur la présence de lat/lon non nulles et non 0/0.
    /// </summary>
    private static bool IsPositionValid(FlespiMessageDto dto)
    {
        if (dto.PositionValid == false) return false;
        if (dto.PositionLatitude is not { } lat || dto.PositionLongitude is not { } lng) return false;
        if (lat == 0 && lng == 0) return false;
        return true;
    }

    private DateTime ParseServerTimestamp(FlespiMessageDto dto, out bool missing)
    {
        if (dto.ServerTimestamp is { } ts)
        {
            missing = false;
            return DateTimeOffset.FromUnixTimeMilliseconds((long)(ts * 1000)).UtcDateTime;
        }
        missing = true;
        return timeProvider.GetUtcNow().UtcDateTime;
    }

    /// <summary>
    /// timestamp (device) à 0/absent, ou écart > 24h avec server.timestamp -> aberrant,
    /// on retient server.timestamp comme device_ts (le point est conservé, pas rejeté).
    /// </summary>
    private (DateTime deviceTsUtc, bool aberrant) ResolveDeviceTimestamp(FlespiMessageDto dto, DateTime serverTsUtc, bool serverTsMissing)
    {
        if (dto.Timestamp is not { } ts || ts <= 0)
            return (serverTsUtc, true);

        DateTime deviceTsUtc = DateTimeOffset.FromUnixTimeMilliseconds((long)(ts * 1000)).UtcDateTime;

        if (!serverTsMissing && (deviceTsUtc - serverTsUtc).Duration() > MaxTimestampDrift)
            return (serverTsUtc, true);

        return (deviceTsUtc, false);
    }

    private static MappingOutcome AnomalyOnly(string? ident, string raison, string rawJson, DateTime nowUtc) =>
        new(null, IngestAnomaly.Create(ident, raison, rawJson, nowUtc));
}
