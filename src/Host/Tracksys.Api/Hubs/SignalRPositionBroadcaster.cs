using Microsoft.AspNetCore.SignalR;
using Tracksys.Modules.Ingestion.Application.Abstractions;
using Tracksys.Modules.Ingestion.Application.Dtos;

namespace Tracksys.Api.Hubs;

public class SignalRPositionBroadcaster(IHubContext<PositionsHub> hubContext) : IPositionBroadcaster
{
    public Task BroadcastAsync(IReadOnlyList<PositionDto> positions, CancellationToken cancellationToken = default) =>
        hubContext.Clients.All.SendAsync("PositionsUpdated", positions, cancellationToken);
}
