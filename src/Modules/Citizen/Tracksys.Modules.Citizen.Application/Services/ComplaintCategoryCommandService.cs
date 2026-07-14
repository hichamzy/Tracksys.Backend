using Tracksys.Modules.Citizen.Application.Abstractions;
using Tracksys.Modules.Citizen.Application.Dtos;
using Tracksys.Modules.Citizen.Domain.Entities;
using Tracksys.Shared.Kernel.Results;

namespace Tracksys.Modules.Citizen.Application.Services;

public class ComplaintCategoryCommandService(ICitizenUnitOfWork unitOfWork)
{
    public async Task<Result<int>> CreateAsync(CreateComplaintCategoryRequest request, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.ComplaintCategories.AnyAsync(c => c.Label == request.Label, cancellationToken))
            return Result.Failure<int>($"Une catégorie '{request.Label}' existe déjà.");

        ComplaintCategory category = ComplaintCategory.Create(request.Label, request.Icon, request.DefaultPriority, request.SlaHours);

        await unitOfWork.ComplaintCategories.AddAsync(category, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(category.Id);
    }

    public async Task<Result> UpdateAsync(int categoryId, UpdateComplaintCategoryRequest request, CancellationToken cancellationToken = default)
    {
        ComplaintCategory? category = await unitOfWork.ComplaintCategories.GetByIdAsync(categoryId, cancellationToken);
        if (category is null) return Result.Failure("Catégorie introuvable.");

        category.UpdateDetails(request.Label, request.Icon, request.DefaultPriority, request.SlaHours);
        unitOfWork.ComplaintCategories.Update(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> SetActiveAsync(int categoryId, bool isActive, CancellationToken cancellationToken = default)
    {
        ComplaintCategory? category = await unitOfWork.ComplaintCategories.GetByIdAsync(categoryId, cancellationToken);
        if (category is null) return Result.Failure("Catégorie introuvable.");

        category.SetActive(isActive);
        unitOfWork.ComplaintCategories.Update(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
