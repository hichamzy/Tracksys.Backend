using Tracksys.Modules.Alerting.Application.Abstractions;
using Tracksys.Modules.Alerting.Domain.Entities;
using Tracksys.Shared.Kernel.Results;

namespace Tracksys.Modules.Alerting.Application.Services;

public class AlertCommandService(IAlertingUnitOfWork unitOfWork)
{
    public async Task<Result> MarkAsReadAsync(long alertId, string userId, CancellationToken cancellationToken = default)
    {
        Alert? alert = await unitOfWork.Alerts.GetByIdAsync(alertId, cancellationToken);
        if (alert is null) return Result.Failure("Alerte introuvable.");

        alert.MarkAsRead(userId);
        unitOfWork.Alerts.Update(alert);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default)
    {
        var unread = await unitOfWork.Alerts.FindAsync(a => a.IsUnread, cancellationToken);
        foreach (Alert alert in unread)
        {
            alert.MarkAsRead(userId);
            unitOfWork.Alerts.Update(alert);
        }
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
