using Tracksys.Modules.Citizen.Application.Abstractions;
using Tracksys.Modules.Citizen.Application.Dtos;

namespace Tracksys.Modules.Citizen.Application.Services;

public class ComplaintCategoryQueryService(ICitizenUnitOfWork unitOfWork)
{
    public async Task<IReadOnlyList<ComplaintCategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var categories = await unitOfWork.ComplaintCategories.GetAllAsync(cancellationToken);

        return categories
            .Select(c => new ComplaintCategoryDto(c.Id, c.Label, c.Icon, c.DefaultPriority, c.SlaHours, c.IsActive))
            .ToList();
    }
}
