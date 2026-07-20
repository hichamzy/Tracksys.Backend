using Tracksys.Modules.Ingestion.Application.Dtos;

namespace Tracksys.Modules.Ingestion.Application.Abstractions;

public interface IPositionQueryService
{
    /// <summary>cityId null (pas de JWT/token sans claim city_id, ou SuperAdmin) = pas de filtre, comportement legacy inchangé.</summary>
    Task<IReadOnlyList<PositionDto>> GetLiveAsync(Guid? cityId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PositionDto>> GetHistoryAsync(string ident, DateTime fromUtc, DateTime toUtc, Guid? cityId, CancellationToken cancellationToken = default);
}
