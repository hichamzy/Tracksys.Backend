using Tracksys.Modules.Ingestion.Application.Dtos;

namespace Tracksys.Modules.Ingestion.Application.Abstractions;

public interface IPositionQueryService
{
    Task<IReadOnlyList<PositionDto>> GetLiveAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PositionDto>> GetHistoryAsync(string ident, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default);
}
