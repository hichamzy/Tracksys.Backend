using Tracksys.Modules.Alerting.Domain.Entities;
using Tracksys.Shared.Kernel.Persistence;
using Tracksys.Shared.Kernel.Repositories;

namespace Tracksys.Modules.Alerting.Application.Abstractions;

public interface IAlertingUnitOfWork : IUnitOfWork
{
    IRepository<Alert, long> Alerts { get; }
    IRepository<AlertRule, int> AlertRules { get; }
    IRepository<AlertType, string> AlertTypes { get; }
}
