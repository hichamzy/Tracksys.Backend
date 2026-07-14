using Tracksys.Modules.Alerting.Application.Abstractions;
using Tracksys.Modules.Alerting.Application.Dtos;

namespace Tracksys.Modules.Alerting.Application.Services;

public class AlertRuleQueryService(IAlertingUnitOfWork unitOfWork)
{
    public async Task<IReadOnlyList<AlertRuleDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var rules = await unitOfWork.AlertRules.GetAllAsync(cancellationToken);
        var alertTypes = await unitOfWork.AlertTypes.GetAllAsync(cancellationToken);
        var alertTypesByCode = alertTypes.ToDictionary(t => t.Id);

        return rules
            .Select(r => new AlertRuleDto(
                r.Id, r.AlertTypeCode,
                alertTypesByCode.TryGetValue(r.AlertTypeCode, out var alertType) ? alertType.Label : r.AlertTypeCode,
                r.IsEnabled, r.Threshold, r.Unit, r.Description))
            .ToList();
    }
}
