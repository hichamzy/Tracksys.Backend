using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tracksys.Modules.Ingestion.Application.Abstractions;
using Tracksys.Modules.Ingestion.Application.Dtos;
using Tracksys.Modules.Ingestion.Application.Mapping;
using Tracksys.Modules.Ingestion.Domain.Entities;

namespace Tracksys.Modules.Ingestion.Application.Services;

/// <summary>
/// Orchestre parse -> map -> écrit -> commit, strictement séquentiel et synchrone.
/// Ne retourne (et donc l'endpoint ne répond 200) qu'après le COMMIT de
/// ITelemetryWriter.WriteBatchAsync — voir la règle "zéro perte" dans CLAUDE.md.
/// Aucun Task.Run fire-and-forget, aucune file en mémoire : un crash après
/// l'appel HTTP mais avant le retour de cette méthode doit laisser Flespi
/// rejouer le batch (pas de 200 renvoyé), jamais silencieusement perdre les points.
/// </summary>
public class IngestFlespiBatchHandler(
    ITelemetryWriter telemetryWriter,
    FlespiMapper mapper,
    IPositionBroadcaster broadcaster,
    ILogger<IngestFlespiBatchHandler> logger)
{
    public async Task<IngestBatchResult> HandleAsync(IReadOnlyList<JsonElement> rawMessages, CancellationToken cancellationToken = default)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        var points = new List<TelemetryPoint>(rawMessages.Count);
        var anomalies = new List<IngestAnomaly>();

        foreach (JsonElement element in rawMessages)
        {
            string rawJson = element.GetRawText();
            FlespiMessageDto? dto;
            try
            {
                dto = element.Deserialize<FlespiMessageDto>();
            }
            catch (JsonException)
            {
                anomalies.Add(IngestAnomaly.Create(null, IngestAnomalyReasons.PositionInvalide, rawJson, DateTime.UtcNow));
                continue;
            }

            if (dto is null) continue;

            MappingOutcome outcome = await mapper.MapAsync(dto, rawJson, cancellationToken);
            if (outcome.Point is not null) points.Add(outcome.Point);
            if (outcome.Anomaly is not null) anomalies.Add(outcome.Anomaly);
        }

        IngestBatchResult result = await telemetryWriter.WriteBatchAsync(points, anomalies, cancellationToken);

        stopwatch.Stop();
        logger.LogInformation(
            "Ingestion Flespi : {Received} messages reçus, {Inserted} insérés, {Duplicates} doublons ignorés, {Anomalies} anomalies, {ElapsedMs} ms",
            result.Received, result.Inserted, result.DuplicatesIgnored, result.AnomaliesLogged, stopwatch.ElapsedMilliseconds);

        // Push temps réel après le COMMIT réussi uniquement — ne fait jamais échouer
        // l'ingestion elle-même (le "zéro perte" prime sur la diffusion live).
        if (points.Count > 0)
        {
            try
            {
                await broadcaster.BroadcastAsync(points.Select(ToPositionDto).ToList(), cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Échec de la diffusion temps réel des positions (ingestion déjà commitée, sans impact).");
            }
        }

        return result;
    }

    private static PositionDto ToPositionDto(TelemetryPoint point) => new(
        point.Ident, point.DeviceTsUtc, point.Latitude, point.Longitude,
        point.PositionSpeed, point.BatteryLevel,
        point.ChariotId, point.ChariotNumero, point.DelegataireId, point.PlanningId, point.CircuitId);
}
