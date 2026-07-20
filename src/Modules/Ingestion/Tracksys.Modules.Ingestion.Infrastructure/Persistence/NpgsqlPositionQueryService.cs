using NetTopologySuite.Geometries;
using Npgsql;
using Tracksys.Modules.Ingestion.Application.Abstractions;
using Tracksys.Modules.Ingestion.Application.Dtos;

namespace Tracksys.Modules.Ingestion.Infrastructure.Persistence;

/// <summary>
/// Lecture seule sur last_position (carte live) et telemetry (historique) — SQL brut, cohérent
/// avec l'écriture. La télémétrie brute n'a pas de city_id propre (voir 002_telemetry.sql,
/// pipeline d'ingestion non tenanté par design) : le filtrage optionnel par ville se fait via un
/// JOIN vers fleet.vehicles/fleet.chariots sur l'ident, uniquement quand cityId est fourni
/// (endpoint public [AllowAnonymous] — cityId null = pas de filtre, comportement legacy inchangé).
/// </summary>
public class NpgsqlPositionQueryService(NpgsqlDataSource dataSource) : IPositionQueryService
{
    private const string LiveSql = """
        SELECT lp.ident, lp.device_ts, lp.position, lp.position_speed, lp.battery_level,
               lp.chariot_id, lp.chariot_numero, lp.delegataire_id, lp.planning_id, lp.circuit_id
        FROM ingestion.last_position lp
        WHERE @city_id::uuid IS NULL
           OR EXISTS (SELECT 1 FROM fleet.vehicles v WHERE v.flespi_ident = lp.ident AND v.city_id = @city_id::uuid)
           OR EXISTS (SELECT 1 FROM fleet.chariots c WHERE c.flespi_ident = lp.ident AND c.city_id = @city_id::uuid)
        """;

    private const string HistorySql = """
        SELECT t.ident, t.device_ts, t.position, t.position_speed, t.battery_level,
               t.chariot_id, t.chariot_numero, t.delegataire_id, t.planning_id, t.circuit_id
        FROM ingestion.telemetry t
        WHERE t.ident = @ident AND t.device_ts BETWEEN @from_ts AND @to_ts
          AND (
            @city_id::uuid IS NULL
            OR EXISTS (SELECT 1 FROM fleet.vehicles v WHERE v.flespi_ident = t.ident AND v.city_id = @city_id::uuid)
            OR EXISTS (SELECT 1 FROM fleet.chariots c WHERE c.flespi_ident = t.ident AND c.city_id = @city_id::uuid)
          )
        ORDER BY t.device_ts
        """;

    public async Task<IReadOnlyList<PositionDto>> GetLiveAsync(Guid? cityId, CancellationToken cancellationToken = default)
    {
        await using NpgsqlConnection connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(LiveSql, connection);
        command.Parameters.AddWithValue("city_id", (object?)cityId ?? DBNull.Value);
        return await ReadAllAsync(command, cancellationToken);
    }

    public async Task<IReadOnlyList<PositionDto>> GetHistoryAsync(string ident, DateTime fromUtc, DateTime toUtc, Guid? cityId, CancellationToken cancellationToken = default)
    {
        await using NpgsqlConnection connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(HistorySql, connection);
        command.Parameters.AddWithValue("ident", ident);
        command.Parameters.AddWithValue("from_ts", fromUtc);
        command.Parameters.AddWithValue("to_ts", toUtc);
        command.Parameters.AddWithValue("city_id", (object?)cityId ?? DBNull.Value);
        return await ReadAllAsync(command, cancellationToken);
    }

    private static async Task<IReadOnlyList<PositionDto>> ReadAllAsync(NpgsqlCommand command, CancellationToken cancellationToken)
    {
        var results = new List<PositionDto>();
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var point = (Point)reader.GetValue(reader.GetOrdinal("position"));
            results.Add(new PositionDto(
                reader.GetString(reader.GetOrdinal("ident")),
                reader.GetDateTime(reader.GetOrdinal("device_ts")),
                point.Y,
                point.X,
                GetNullable<float>(reader, "position_speed"),
                GetNullable<float>(reader, "battery_level"),
                GetNullable<int>(reader, "chariot_id"),
                reader.IsDBNull(reader.GetOrdinal("chariot_numero")) ? null : reader.GetString(reader.GetOrdinal("chariot_numero")),
                GetNullable<int>(reader, "delegataire_id"),
                GetNullable<long>(reader, "planning_id"),
                GetNullable<int>(reader, "circuit_id")));
        }
        return results;
    }

    private static T? GetNullable<T>(NpgsqlDataReader reader, string column) where T : struct
    {
        int ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetFieldValue<T>(ordinal);
    }
}
