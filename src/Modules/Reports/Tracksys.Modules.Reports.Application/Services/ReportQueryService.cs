using Tracksys.Modules.Alerting.Application.Abstractions;
using Tracksys.Modules.Citizen.Application.Abstractions;
using Tracksys.Modules.Citizen.Domain.Enums;
using Tracksys.Modules.Fleet.Application.Abstractions;
using Tracksys.Modules.Reports.Application.Abstractions;
using Tracksys.Modules.Reports.Application.Dtos;

namespace Tracksys.Modules.Reports.Application.Services;

public class ReportQueryService(
    IReportsUnitOfWork reportsUnitOfWork,
    IFleetUnitOfWork fleetUnitOfWork,
    ICitizenUnitOfWork citizenUnitOfWork,
    IAlertingUnitOfWork alertingUnitOfWork)
{
    public async Task<ReportKpisDto> GetKpisAsync(CancellationToken cancellationToken = default)
    {
        var vehicles = await fleetUnitOfWork.Vehicles.GetAllAsync(cancellationToken);
        var complaints = await citizenUnitOfWork.Complaints.GetAllAsync(cancellationToken);
        var alerts = await alertingUnitOfWork.Alerts.GetAllAsync(cancellationToken);

        decimal totalDistance = vehicles.Sum(v => v.DistanceTodayKm);
        int resolvedComplaints = complaints.Count(c => c.Status == ComplaintStatus.Resolved);

        return new ReportKpisDto(totalDistance, resolvedComplaints, complaints.Count, alerts.Count);
    }

    public async Task<IReadOnlyList<MonthlySeriesPointDto>> GetDistanceSeriesAsync(CancellationToken cancellationToken = default)
    {
        var stats = await reportsUnitOfWork.FleetMonthlyStats.GetAllAsync(cancellationToken);
        return stats
            .OrderBy(s => s.YearMonth)
            .TakeLast(12)
            .Select(s => new MonthlySeriesPointDto(s.YearMonth, s.TotalDistanceKm))
            .ToList();
    }

    public async Task<IReadOnlyList<MonthlySeriesPointDto>> GetResolutionSeriesAsync(CancellationToken cancellationToken = default)
    {
        var stats = await reportsUnitOfWork.FleetMonthlyStats.GetAllAsync(cancellationToken);
        return stats
            .OrderBy(s => s.YearMonth)
            .TakeLast(12)
            .Select(s => new MonthlySeriesPointDto(s.YearMonth, s.ResolutionRatePct))
            .ToList();
    }

    public async Task<IReadOnlyList<SavedReportDto>> GetSavedReportsAsync(CancellationToken cancellationToken = default)
    {
        var savedReports = await reportsUnitOfWork.SavedReports.GetAllAsync(cancellationToken);
        var reportTypes = await reportsUnitOfWork.ReportTypes.GetAllAsync(cancellationToken);
        var reportTypesById = reportTypes.ToDictionary(t => t.Id);

        return savedReports
            .OrderByDescending(r => r.GeneratedAtUtc)
            .Select(r => new SavedReportDto(
                r.Id,
                reportTypesById.TryGetValue(r.ReportTypeId, out var type) ? type.Label : "—",
                r.Name, r.PeriodLabel, r.Format, r.FileUrl, r.GeneratedAtUtc))
            .ToList();
    }

    public async Task<IReadOnlyList<ReportTypeDto>> GetReportTypesAsync(CancellationToken cancellationToken = default)
    {
        var reportTypes = await reportsUnitOfWork.ReportTypes.GetAllAsync(cancellationToken);
        return reportTypes.Select(t => new ReportTypeDto(t.Id, t.Label)).ToList();
    }
}
