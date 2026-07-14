namespace Tracksys.Modules.Ingestion.Application.Options;

public class IngestionOptions
{
    public const string SectionName = "Ingestion";

    /// <summary>Clé partagée attendue en query string (?key=...) sur POST /api/ingest/flespi. Générée aléatoirement, jamais en dur.</summary>
    public string ApiKey { get; set; } = string.Empty;
}
