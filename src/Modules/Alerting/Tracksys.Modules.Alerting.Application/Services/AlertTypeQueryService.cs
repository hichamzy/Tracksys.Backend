using Tracksys.Modules.Alerting.Application.Abstractions;
using Tracksys.Modules.Alerting.Application.Dtos;

namespace Tracksys.Modules.Alerting.Application.Services;

public class AlertTypeQueryService(IAlertingUnitOfWork unitOfWork)
{
    public async Task<IReadOnlyList<AlertTypeDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var alertTypes = await unitOfWork.AlertTypes.GetAllAsync(cancellationToken);

        return alertTypes
            .Select(t => new AlertTypeDto(t.Id, t.Label, t.Severity))
            .ToList();
    }
}
