using Tracksys.Modules.Citizen.Application.Abstractions;
using Tracksys.Modules.Citizen.Application.Dtos;
using Tracksys.Modules.Citizen.Domain.Entities;
using Tracksys.Shared.Kernel.Results;

namespace Tracksys.Modules.Citizen.Application.Services;

public class ComplaintCommandService(ICitizenUnitOfWork unitOfWork)
{
    public async Task<Result> AssignVehicleAsync(int complaintId, int vehicleId, CancellationToken cancellationToken = default)
    {
        Complaint? complaint = await unitOfWork.Complaints.GetByIdAsync(complaintId, cancellationToken);
        if (complaint is null) return Result.Failure("Réclamation introuvable.");

        complaint.AssignVehicle(vehicleId);
        unitOfWork.Complaints.Update(complaint);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ResolveAsync(int complaintId, string? photoAfterUrl, CancellationToken cancellationToken = default)
    {
        Complaint? complaint = await unitOfWork.Complaints.GetByIdAsync(complaintId, cancellationToken);
        if (complaint is null) return Result.Failure("Réclamation introuvable.");

        complaint.Resolve(photoAfterUrl);
        unitOfWork.Complaints.Update(complaint);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
