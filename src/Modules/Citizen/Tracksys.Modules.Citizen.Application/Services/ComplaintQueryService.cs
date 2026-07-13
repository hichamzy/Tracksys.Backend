using Tracksys.Modules.Citizen.Application.Abstractions;
using Tracksys.Modules.Citizen.Application.Dtos;

namespace Tracksys.Modules.Citizen.Application.Services;

public class ComplaintQueryService(ICitizenUnitOfWork unitOfWork)
{
    public async Task<IReadOnlyList<ComplaintDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var complaints = await unitOfWork.Complaints.GetAllAsync(cancellationToken);

        return complaints
            .Select(c => new ComplaintDto(
                c.Id, c.Code, c.CategoryId, c.Priority, c.Status.ToString(),
                c.ZoneLabel, c.Lat, c.Lng, c.AssignedVehicleId, c.ReportedAtUtc))
            .ToList();
    }
}
