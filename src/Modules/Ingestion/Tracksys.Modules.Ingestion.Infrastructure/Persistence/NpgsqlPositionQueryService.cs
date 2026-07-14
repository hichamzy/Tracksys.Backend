using NetTopologySuite.Geometries;
using Npgsql;
using Tracksys.Modules.Ingestion.Application.Abstractions;
using Tracksys.Modules.Ingestion.Application.Dtos;

namespace Tracksys.Modules.Ingestion.Infrastructure.Persistence;

/// <summary>Lecture seule sur last_position (carte live) et telemetry (historique) — SQL brut, cohérent avec l'écriture.</summary>
public class NpgsqlPositionQueryService(NpgsqlDataSource dataSource) : IPositionQueryService
{
    private const string LiveSql = """
        SELECT ident, device_ts, position, position_speed, battery_level,
               chariot_id, chariot_numero, delegataire_id, planning_id, circuit_id
        FROM ingestion.last_position
        """;

    private const string HistorySql = """
        SELECT ident, device_ts, position, position_speed, battery_level,
               chariot_id, chariot_numero, delegataire_id, planning_id, circuit_id
        FROM ingestion.telemetry
        WHERE ident = @ident AND device_ts BETWEEN @from_ts AND @to_ts
        ORDER BY device_ts
        """;

    public async Task<IReadOnlyList<PositionDto>> GetLiveAsync(CancellationToken cancellationToken = default)
    {
        await using NpgsqlConnection connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(LiveSql, connection);
        return await ReadAllAsync(command, cancellationToken);
    }

    public async Task<IReadOnlyList<PositionDto>> GetHistoryAsync(string ident, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
    {
        await using NpgsqlConnection connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(HistorySql, connection);
        command.Parameters.AddWithValue("ident", ident);
        command.Parameters.AddWithValue("from_ts", fromUtc);
        command.Parameters.AddWithValue("to_ts", toUtc);
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
