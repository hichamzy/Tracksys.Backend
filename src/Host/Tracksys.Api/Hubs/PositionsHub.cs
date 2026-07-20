using Microsoft.AspNetCore.SignalR;

namespace Tracksys.Api.Hubs;

/// <summary>
/// Hub public (même politique que GET /api/positions/live — carte live publique par design).
/// Les clients se contentent d'écouter l'événement "PositionsUpdated" ; aucune méthode
/// serveur exposée aux clients pour l'instant (diffusion uniquement, pas d'interaction).
///
/// Diffusion non filtrée par ville (Clients.All), volontairement : router par groupe de ville
/// exigerait de résoudre city_id dans IngestFlespiBatchHandler, le pipeline d'ingestion Flespi
/// zéro-perte/synchrone qui ne doit recevoir aucune modification liée au tenant (voir
/// docs/plan multi-tenant, section ingestion). Le filtrage par ville reste appliqué côté REST
/// (GET /api/positions/live|history) quand un JWT est fourni — le front recroise les positions
/// broadcastées avec la liste REST déjà filtrée par ville.
/// </summary>
public class PositionsHub : Hub;
