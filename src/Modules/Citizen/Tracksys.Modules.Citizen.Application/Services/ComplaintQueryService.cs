using Tracksys.Modules.Citizen.Application.Abstractions;
using Tracksys.Modules.Citizen.Application.Dtos;

namespace Tracksys.Modules.Citizen.Application.Services;

public class ComplaintQueryService(ICitizenUnitOfWork unitOfWork)
{
    public async Task<IReadOnlyList<ComplaintDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var complaints = await unitOfWork.Complaints.GetAllAsync(cancellationToken);
        var categories = await unitOfWork.ComplaintCategories.GetAllAsync(cancellationToken);
        var categoriesById = categories.ToDictionary(c => c.Id);

        return complaints
            .Select(c => new ComplaintDto(
                c.Id, c.Code, c.CategoryId,
                categoriesById.TryGetValue(c.CategoryId, out var category) ? category.Label : "—",
                c.Priority, c.Status.ToString(), c.ZoneLabel, c.Lat, c.Lng,
                c.AssignedVehicleId, c.ReporterName, c.ReportedAtUtc, c.ResolvedAtUtc,
                c.PhotoBeforeUrl, c.PhotoAfterUrl))
            .ToList();
    }

    public async Task<IReadOnlyList<ComplaintCategoryBreakdownDto>> GetCategoryBreakdownAsync(CancellationToken cancellationToken = default)
    {
        var complaints = await unitOfWork.Complaints.GetAllAsync(cancellationToken);
        var categories = await unitOfWork.ComplaintCategories.GetAllAsync(cancellationToken);
        var total = complaints.Count;

        return categories
            .Select(category =>
            {
                var count = complaints.Count(c => c.CategoryId == category.Id);
                var percentage = total == 0 ? 0m : Math.Round(100m * count / total, 1);
                return new ComplaintCategoryBreakdownDto(category.Id, category.Label, count, percentage);
            })
            .Where(b => b.Count > 0)
            .OrderByDescending(b => b.Count)
            .ToList();
    }
}
