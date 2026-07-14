using Tracksys.Modules.Reports.Domain.Entities;
using Tracksys.Shared.Kernel.Persistence;
using Tracksys.Shared.Kernel.Repositories;

namespace Tracksys.Modules.Reports.Application.Abstractions;

public interface IReportsUnitOfWork : IUnitOfWork
{
    IRepository<SavedReport, int> SavedReports { get; }
    IRepository<ReportType, int> ReportTypes { get; }
    IRepository<FleetMonthlyStat, int> FleetMonthlyStats { get; }
}
