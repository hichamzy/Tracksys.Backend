using Microsoft.AspNetCore.SignalR;

namespace Tracksys.Api.Hubs;

/// <summary>
/// Hub public (même politique que GET /api/positions/live — carte live publique par design).
/// Les clients se contentent d'écouter l'événement "PositionsUpdated" ; aucune méthode
/// serveur exposée aux clients pour l'instant (diffusion uniquement, pas d'interaction).
/// </summary>
public class PositionsHub : Hub;
