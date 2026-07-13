using Tracksys.Modules.Alerting.Application.Abstractions;
using Tracksys.Modules.Alerting.Domain.Entities;
using Tracksys.Shared.Infrastructure.Persistence;
using Tracksys.Shared.Kernel.Repositories;

namespace Tracksys.Modules.Alerting.Infrastructure.Persistence;

public class AlertingUnitOfWork : UnitOfWork<AlertingDbContext>, IAlertingUnitOfWork
{
    public IRepository<Alert, long> Alerts { get; }
    public IRepository<AlertRule, int> AlertRules { get; }
    public IRepository<AlertType, string> AlertTypes { get; }

    public AlertingUnitOfWork(AlertingDbContext dbContext) : base(dbContext)
    {
        Alerts = new Repository<Alert, long>(dbContext);
        AlertRules = new Repository<AlertRule, int>(dbContext);
        AlertTypes = new Repository<AlertType, string>(dbContext);
    }
}
