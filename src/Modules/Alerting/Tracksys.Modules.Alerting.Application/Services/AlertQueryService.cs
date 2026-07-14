using Tracksys.Modules.Alerting.Application.Abstractions;
using Tracksys.Modules.Alerting.Application.Dtos;

namespace Tracksys.Modules.Alerting.Application.Services;

public class AlertQueryService(IAlertingUnitOfWork unitOfWork)
{
    public async Task<IReadOnlyList<AlertDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var alerts = await unitOfWork.Alerts.GetAllAsync(cancellationToken);
        var alertTypes = await unitOfWork.AlertTypes.GetAllAsync(cancellationToken);
        var alertTypesByCode = alertTypes.ToDictionary(t => t.Id);

        return alerts
            .OrderByDescending(a => a.OccurredAtUtc)
            .Select(a =>
            {
                alertTypesByCode.TryGetValue(a.AlertTypeCode, out var alertType);
                return new AlertDto(
                    a.Id, a.Code, a.AlertTypeCode, alertType?.Label ?? a.AlertTypeCode, alertType?.Severity ?? "md",
                    a.VehicleId, a.DetailText, a.OccurredAtUtc, a.IsUnread, a.ReadAtUtc, a.ReadByUserId);
            })
            .ToList();
    }
}
