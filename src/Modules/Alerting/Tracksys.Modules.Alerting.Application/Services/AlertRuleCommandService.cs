using Tracksys.Modules.Alerting.Application.Abstractions;
using Tracksys.Modules.Alerting.Domain.Entities;
using Tracksys.Shared.Kernel.Results;

namespace Tracksys.Modules.Alerting.Application.Services;

public class AlertRuleCommandService(IAlertingUnitOfWork unitOfWork)
{
    public async Task<Result> UpdateThresholdAsync(int ruleId, decimal threshold, CancellationToken cancellationToken = default)
    {
        AlertRule? rule = await unitOfWork.AlertRules.GetByIdAsync(ruleId, cancellationToken);
        if (rule is null) return Result.Failure("Règle introuvable.");

        rule.UpdateThreshold(threshold);
        unitOfWork.AlertRules.Update(rule);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ToggleAsync(int ruleId, bool enabled, CancellationToken cancellationToken = default)
    {
        AlertRule? rule = await unitOfWork.AlertRules.GetByIdAsync(ruleId, cancellationToken);
        if (rule is null) return Result.Failure("Règle introuvable.");

        rule.Toggle(enabled);
        unitOfWork.AlertRules.Update(rule);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
