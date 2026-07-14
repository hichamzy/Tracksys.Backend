using Tracksys.Modules.Ingestion.Application.Dtos;

namespace Tracksys.Modules.Ingestion.Application.Abstractions;

/// <summary>
/// Pousse les positions fraîchement écrites aux clients temps réel (SignalR côté Host).
/// Abstraction volontaire : Ingestion.Application ne référence pas SignalR directement
/// (implémentation dans le Host, seul endroit qui compose le pipeline ASP.NET Core).
/// </summary>
public interface IPositionBroadcaster
{
    Task BroadcastAsync(IReadOnlyList<PositionDto> positions, CancellationToken cancellationToken = default);
}
