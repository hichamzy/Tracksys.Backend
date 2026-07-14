using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Tracksys.Modules.Ingestion.Application.Options;
using Tracksys.Modules.Ingestion.Application.Services;

namespace Tracksys.Modules.Ingestion.Api.Controllers;

/// <summary>
/// Reçoit le HTTP Stream Flespi (POST, tableau JSON de messages). Endpoint anonyme
/// (Flespi n'envoie pas de JWT) protégé par une clé partagée en query string,
/// comparée en temps constant. Ne JAMAIS logger l'URL complète (la clé y figure).
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/ingest/flespi")]
public class IngestController(
    IngestFlespiBatchHandler handler,
    IOptions<IngestionOptions> ingestionOptions,
    ILogger<IngestController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Ingest([FromQuery] string? key, CancellationToken cancellationToken)
    {
        if (!IsKeyValid(key))
        {
            logger.LogWarning("Ingestion Flespi : clé invalide ou absente (URL non loggée)");
            return Unauthorized();
        }

        JsonElement[] messages;
        try
        {
            using JsonDocument document = await JsonDocument.ParseAsync(Request.Body, cancellationToken: cancellationToken);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
                return BadRequest("Le corps de la requête doit être un tableau JSON.");

            messages = document.RootElement.EnumerateArray().Select(e => e.Clone()).ToArray();
        }
        catch (JsonException)
        {
            return BadRequest("JSON invalide.");
        }

        // Aucun Task.Run, aucune file : HandleAsync écrit en base (COMMIT inclus)
        // avant que ce endpoint ne réponde. Une exception ici -> 500, Flespi rejoue.
        var result = await handler.HandleAsync(messages, cancellationToken);

        return Ok(result);
    }

    private bool IsKeyValid(string? key)
    {
        string expected = ingestionOptions.Value.ApiKey;
        if (string.IsNullOrEmpty(expected) || string.IsNullOrEmpty(key)) return false;

        byte[] expectedBytes = Encoding.UTF8.GetBytes(expected);
        byte[] actualBytes = Encoding.UTF8.GetBytes(key);
        if (expectedBytes.Length != actualBytes.Length) return false;

        return CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes);
    }
}
