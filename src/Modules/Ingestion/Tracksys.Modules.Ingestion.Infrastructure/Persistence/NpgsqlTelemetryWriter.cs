using NetTopologySuite.Geometries;
using Npgsql;
using NpgsqlTypes;
using Tracksys.Modules.Ingestion.Application.Abstractions;
using Tracksys.Modules.Ingestion.Domain.Entities;

namespace Tracksys.Modules.Ingestion.Infrastructure.Persistence;

/// <summary>
/// Écrit un batch de télémétrie dans UNE SEULE transaction Npgsql :
///   1. COPY binaire vers une table temporaire
///   2. INSERT ... SELECT ... ON CONFLICT (ident, device_ts) DO NOTHING -> telemetry (idempotence rejeu Flespi)
///   3. UPSERT -> last_position, avec WHERE last_position.device_ts < EXCLUDED.device_ts
///      (empêche un rejeu tardif d'écraser une position plus récente déjà en base)
///   4. INSERT anomalies (si présentes)
///   5. COMMIT
/// Ne retourne qu'après le COMMIT — voir IngestFlespiBatchHandler pour la règle "zéro perte".
/// </summary>
public class NpgsqlTelemetryWriter(NpgsqlDataSource dataSource) : ITelemetryWriter
{
    private const string CreateTempTableSql = """
        CREATE TEMP TABLE telemetry_staging (
            ident TEXT NOT NULL,
            device_ts TIMESTAMPTZ NOT NULL,
            server_ts TIMESTAMPTZ NOT NULL,
            position GEOGRAPHY(POINT, 4326) NOT NULL,
            position_speed REAL,
            battery_level REAL,
            battery_voltage REAL,
            is_powerbank_connected BOOLEAN,
            chariot_id INT,
            chariot_numero TEXT,
            boitier_id INT,
            delegataire_id INT,
            planning_id BIGINT,
            circuit_id INT,
            type_prestation_id INT
        ) ON COMMIT DROP
        """;

    private const string CopySql = """
        COPY telemetry_staging (
            ident, device_ts, server_ts, position, position_speed, battery_level,
            battery_voltage, is_powerbank_connected, chariot_id, chariot_numero,
            boitier_id, delegataire_id, planning_id, circuit_id, type_prestation_id
        ) FROM STDIN (FORMAT BINARY)
        """;

    private const string InsertTelemetrySql = """
        INSERT INTO ingestion.telemetry (
            ident, device_ts, server_ts, position, position_speed, battery_level,
            battery_voltage, is_powerbank_connected, chariot_id, chariot_numero,
            boitier_id, delegataire_id, planning_id, circuit_id, type_prestation_id
        )
        SELECT ident, device_ts, server_ts, position, position_speed, battery_level,
               battery_voltage, is_powerbank_connected, chariot_id, chariot_numero,
               boitier_id, delegataire_id, planning_id, circuit_id, type_prestation_id
        FROM telemetry_staging
        ON CONFLICT (ident, device_ts) DO NOTHING
        """;

    private const string UpsertLastPositionSql = """
        INSERT INTO ingestion.last_position (
            ident, device_ts, server_ts, position, position_speed, battery_level,
            battery_voltage, is_powerbank_connected, chariot_id, chariot_numero,
            boitier_id, delegataire_id, planning_id, circuit_id, type_prestation_id
        )
        SELECT DISTINCT ON (ident)
               ident, device_ts, server_ts, position, position_speed, battery_level,
               battery_voltage, is_powerbank_connected, chariot_id, chariot_numero,
               boitier_id, delegataire_id, planning_id, circuit_id, type_prestation_id
        FROM telemetry_staging
        ORDER BY ident, device_ts DESC
        ON CONFLICT (ident) DO UPDATE SET
            device_ts = EXCLUDED.device_ts,
            server_ts = EXCLUDED.server_ts,
            position = EXCLUDED.position,
            position_speed = EXCLUDED.position_speed,
            battery_level = EXCLUDED.battery_level,
            battery_voltage = EXCLUDED.battery_voltage,
            is_powerbank_connected = EXCLUDED.is_powerbank_connected,
            chariot_id = EXCLUDED.chariot_id,
            chariot_numero = EXCLUDED.chariot_numero,
            boitier_id = EXCLUDED.boitier_id,
            delegataire_id = EXCLUDED.delegataire_id,
            planning_id = EXCLUDED.planning_id,
            circuit_id = EXCLUDED.circuit_id,
            type_prestation_id = EXCLUDED.type_prestation_id
        WHERE ingestion.last_position.device_ts < EXCLUDED.device_ts
        """;

    private const string InsertAnomalySql = """
        INSERT INTO ingestion.ingest_anomaly (time, ident, raison, payload_brut)
        VALUES (@time, @ident, @raison, @payload::jsonb)
        """;

    private static readonly GeometryFactory GeometryFactory = new(new PrecisionModel(), 4326);

    public async Task<IngestBatchResult> WriteBatchAsync(
        IReadOnlyList<TelemetryPoint> points,
        IReadOnlyList<IngestAnomaly> anomalies,
        CancellationToken cancellationToken = default)
    {
        int received = points.Count + anomalies.Count;

        await using NpgsqlConnection connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);

        int inserted = 0;
        int duplicatesIgnored = 0;

        if (points.Count > 0)
        {
            await using (var createTemp = new NpgsqlCommand(CreateTempTableSql, connection, transaction))
                await createTemp.ExecuteNonQueryAsync(cancellationToken);

            await using (NpgsqlBinaryImporter importer = await connection.BeginBinaryImportAsync(CopySql, cancellationToken))
            {
                foreach (TelemetryPoint point in points)
                {
                    await importer.StartRowAsync(cancellationToken);
                    await importer.WriteAsync(point.Ident, NpgsqlDbType.Text, cancellationToken);
                    await importer.WriteAsync(point.DeviceTsUtc, NpgsqlDbType.TimestampTz, cancellationToken);
                    await importer.WriteAsync(point.ServerTsUtc, NpgsqlDbType.TimestampTz, cancellationToken);
                    await importer.WriteAsync(GeometryFactory.CreatePoint(new Coordinate(point.Longitude, point.Latitude)), NpgsqlDbType.Geography, cancellationToken);
                    await WriteNullableAsync(importer, point.PositionSpeed, NpgsqlDbType.Real, cancellationToken);
                    await WriteNullableAsync(importer, point.BatteryLevel, NpgsqlDbType.Real, cancellationToken);
                    await WriteNullableAsync(importer, point.BatteryVoltage, NpgsqlDbType.Real, cancellationToken);
                    await WriteNullableAsync(importer, point.IsPowerbankConnected, NpgsqlDbType.Boolean, cancellationToken);
                    await WriteNullableAsync(importer, point.ChariotId, NpgsqlDbType.Integer, cancellationToken);
                    await WriteNullableAsync(importer, point.ChariotNumero, NpgsqlDbType.Text, cancellationToken);
                    await WriteNullableAsync(importer, point.BoitierId, NpgsqlDbType.Integer, cancellationToken);
                    await WriteNullableAsync(importer, point.DelegataireId, NpgsqlDbType.Integer, cancellationToken);
                    await WriteNullableAsync(importer, point.PlanningId, NpgsqlDbType.Bigint, cancellationToken);
                    await WriteNullableAsync(importer, point.CircuitId, NpgsqlDbType.Integer, cancellationToken);
                    await WriteNullableAsync(importer, point.TypePrestationId, NpgsqlDbType.Integer, cancellationToken);
                }
                await importer.CompleteAsync(cancellationToken);
            }

            await using (var insertTelemetry = new NpgsqlCommand(InsertTelemetrySql, connection, transaction))
            {
                inserted = await insertTelemetry.ExecuteNonQueryAsync(cancellationToken);
                duplicatesIgnored = points.Count - inserted;
            }

            await using (var upsertLastPosition = new NpgsqlCommand(UpsertLastPositionSql, connection, transaction))
                await upsertLastPosition.ExecuteNonQueryAsync(cancellationToken);
        }

        foreach (IngestAnomaly anomaly in anomalies)
        {
            await using var insertAnomaly = new NpgsqlCommand(InsertAnomalySql, connection, transaction);
            insertAnomaly.Parameters.AddWithValue("time", anomaly.TimeUtc);
            insertAnomaly.Parameters.AddWithValue("ident", (object?)anomaly.Ident ?? DBNull.Value);
            insertAnomaly.Parameters.AddWithValue("raison", anomaly.Raison);
            insertAnomaly.Parameters.AddWithValue("payload", anomaly.PayloadBrut);
            await insertAnomaly.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);

        return new IngestBatchResult(received, inserted, duplicatesIgnored, anomalies.Count);
    }

    private static async Task WriteNullableAsync<T>(NpgsqlBinaryImporter importer, T? value, NpgsqlDbType dbType, CancellationToken cancellationToken)
        where T : struct
    {
        if (value.HasValue) await importer.WriteAsync(value.Value, dbType, cancellationToken);
        else await importer.WriteNullAsync(cancellationToken);
    }

    private static async Task WriteNullableAsync(NpgsqlBinaryImporter importer, string? value, NpgsqlDbType dbType, CancellationToken cancellationToken)
    {
        if (value is not null) await importer.WriteAsync(value, dbType, cancellationToken);
        else await importer.WriteNullAsync(cancellationToken);
    }
}
