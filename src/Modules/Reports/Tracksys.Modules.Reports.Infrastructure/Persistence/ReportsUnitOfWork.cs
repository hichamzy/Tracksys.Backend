using Tracksys.Modules.Reports.Application.Abstractions;
using Tracksys.Modules.Reports.Domain.Entities;
using Tracksys.Shared.Infrastructure.Persistence;
using Tracksys.Shared.Kernel.Repositories;

namespace Tracksys.Modules.Reports.Infrastructure.Persistence;

public class ReportsUnitOfWork : UnitOfWork<ReportsDbContext>, IReportsUnitOfWork
{
    public IRepository<SavedReport, int> SavedReports { get; }
    public IRepository<ReportType, int> ReportTypes { get; }
    public IRepository<FleetMonthlyStat, int> FleetMonthlyStats { get; }

    public ReportsUnitOfWork(ReportsDbContext dbContext) : base(dbContext)
    {
        SavedReports = new Repository<SavedReport, int>(dbContext);
        ReportTypes = new Repository<ReportType, int>(dbContext);
        FleetMonthlyStats = new Repository<FleetMonthlyStat, int>(dbContext);
    }
}
