using Microsoft.EntityFrameworkCore;
using Npgsql;
using Tracksys.Modules.Ingestion.Application.Abstractions;

namespace Tracksys.Modules.Ingestion.Infrastructure.Lookup;

/// <summary>
/// Jointure directe fleet.vehicles / fleet.chariots / fleet.plannings, base unique
/// (pas de table ref_* dupliquée — voir gabarit de requête en tête de
/// database/postgresql/002_telemetry.sql). SQL brut via Npgsql plutôt qu'EF Core
/// cross-module : évite de faire dépendre ce module d'un DbContext Fleet complet
/// pour une seule requête de lecture.
/// </summary>
public class LookupService(NpgsqlDataSource dataSource) : ILookupService
{
    private const string Sql = """
        SELECT
            v.id AS vehicle_id,
            c.id AS chariot_id,
            c.numero AS chariot_numero,
            c.boitier_id,
            c.delegataire_id,
            p.id AS planning_id,
            p.circuit_id,
            p.type_prestation_id
        FROM (SELECT @ident::text AS ident) AS src
        LEFT JOIN fleet.vehicles v ON v.flespi_ident = src.ident
        LEFT JOIN fleet.chariots c ON c.flespi_ident = src.ident
        LEFT JOIN fleet.plannings p ON p.chariot_id = c.id
                                    AND @device_ts BETWEEN p.debut_utc AND p.fin_utc
        """;

    public async Task<LookupResult> ResolveAsync(string ident, DateTime deviceTsUtc, CancellationToken cancellationToken = default)
    {
        await using NpgsqlConnection connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(Sql, connection);
        command.Parameters.AddWithValue("ident", ident);
        command.Parameters.AddWithValue("device_ts", deviceTsUtc);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return new LookupResult(false, false, null, null, null, null, null, null, null);

        bool vehicleMatched = !reader.IsDBNull(reader.GetOrdinal("vehicle_id"));
        bool chariotMatched = !reader.IsDBNull(reader.GetOrdinal("chariot_id"));

        return new LookupResult(
            VehicleMatched: vehicleMatched,
            ChariotMatched: chariotMatched,
            ChariotId: chariotMatched ? reader.GetInt32(reader.GetOrdinal("chariot_id")) : null,
            ChariotNumero: reader.IsDBNull(reader.GetOrdinal("chariot_numero")) ? null : reader.GetString(reader.GetOrdinal("chariot_numero")),
            BoitierId: reader.IsDBNull(reader.GetOrdinal("boitier_id")) ? null : reader.GetInt32(reader.GetOrdinal("boitier_id")),
            DelegataireId: reader.IsDBNull(reader.GetOrdinal("delegataire_id")) ? null : reader.GetInt32(reader.GetOrdinal("delegataire_id")),
            PlanningId: reader.IsDBNull(reader.GetOrdinal("planning_id")) ? null : reader.GetInt64(reader.GetOrdinal("planning_id")),
            CircuitId: reader.IsDBNull(reader.GetOrdinal("circuit_id")) ? null : reader.GetInt32(reader.GetOrdinal("circuit_id")),
            TypePrestationId: reader.IsDBNull(reader.GetOrdinal("type_prestation_id")) ? null : reader.GetInt32(reader.GetOrdinal("type_prestation_id")));
    }
}
