using System.Text.Json.Serialization;

namespace Tracksys.Modules.Ingestion.Application.Dtos;

/// <summary>
/// Un message du tableau JSON envoyé par le HTTP Stream Flespi. Champs à plat,
/// noms à points — seuls les champs du schéma cible sont mappés ; tout le reste
/// du payload (~40 champs can.*, custom.param.NNN pour la famille Teltonika,
/// alarm.*, sim.*, etc.) est délibérément ignoré, jamais stocké en JSONB fourre-tout.
/// </summary>
public sealed class FlespiMessageDto
{
    [JsonPropertyName("ident")]
    public string? Ident { get; init; }

    [JsonPropertyName("timestamp")]
    public double? Timestamp { get; init; }

    [JsonPropertyName("server.timestamp")]
    public double? ServerTimestamp { get; init; }

    [JsonPropertyName("position.latitude")]
    public double? PositionLatitude { get; init; }

    [JsonPropertyName("position.longitude")]
    public double? PositionLongitude { get; init; }

    [JsonPropertyName("position.speed")]
    public float? PositionSpeed { get; init; }

    [JsonPropertyName("position.valid")]
    public bool? PositionValid { get; init; }

    [JsonPropertyName("battery.level")]
    public float? BatteryLevel { get; init; }

    [JsonPropertyName("battery.voltage")]
    public float? BatteryVoltage { get; init; }
}
