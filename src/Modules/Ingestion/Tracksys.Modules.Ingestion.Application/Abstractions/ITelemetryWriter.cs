using Tracksys.Modules.Ingestion.Domain.Entities;

namespace Tracksys.Modules.Ingestion.Application.Abstractions;

public record IngestBatchResult(int Received, int Inserted, int DuplicatesIgnored, int AnomaliesLogged);

/// <summary>
/// Écrit un batch de télémétrie + les anomalies associées dans UNE SEULE transaction
/// Npgsql (COPY table temp -> INSERT ON CONFLICT DO NOTHING -> UPSERT last_position).
/// Ne répond jamais avant le COMMIT — voir IngestFlespiBatchHandler.
/// Volontairement hors de IRepository/IUnitOfWork génériques : NpgsqlBinaryImporter et
/// la transaction multi-étapes ne rentrent pas dans cette abstraction EF Core.
/// </summary>
public interface ITelemetryWriter
{
    Task<IngestBatchResult> WriteBatchAsync(
        IReadOnlyList<TelemetryPoint> points,
        IReadOnlyList<IngestAnomaly> anomalies,
        CancellationToken cancellationToken = default);
}
