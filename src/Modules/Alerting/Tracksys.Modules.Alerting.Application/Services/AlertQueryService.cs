using Tracksys.Modules.Alerting.Application.Abstractions;
using Tracksys.Modules.Alerting.Application.Dtos;

namespace Tracksys.Modules.Alerting.Application.Services;

public class AlertQueryService(IAlertingUnitOfWork unitOfWork)
{
    public async Task<IReadOnlyList<AlertDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var alerts = await unitOfWork.Alerts.GetAllAsync(cancellationToken);

        return alerts
            .OrderByDescending(a => a.OccurredAtUtc)
            .Select(a => new AlertDto(a.Id, a.Code, a.AlertTypeCode, a.VehicleId, a.DetailText, a.OccurredAtUtc, a.IsUnread))
            .ToList();
    }
}
