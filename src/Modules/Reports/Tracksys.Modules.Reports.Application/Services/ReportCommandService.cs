using Tracksys.Modules.Reports.Application.Abstractions;
using Tracksys.Modules.Reports.Application.Dtos;
using Tracksys.Modules.Reports.Domain.Entities;
using Tracksys.Shared.Kernel.Results;

namespace Tracksys.Modules.Reports.Application.Services;

public class ReportCommandService(IReportsUnitOfWork unitOfWork)
{
    public async Task<Result<int>> SaveAsync(CreateSavedReportRequest request, string? userId, CancellationToken cancellationToken = default)
    {
        if (!await unitOfWork.ReportTypes.AnyAsync(t => t.Id == request.ReportTypeId, cancellationToken))
            return Result.Failure<int>("Type de rapport introuvable.");

        SavedReport report = SavedReport.Create(request.ReportTypeId, request.Name, request.PeriodLabel, request.Format, userId);

        await unitOfWork.SavedReports.AddAsync(report, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(report.Id);
    }
}
