using Tracksys.Modules.Citizen.Domain.Entities;
using Tracksys.Shared.Kernel.Persistence;
using Tracksys.Shared.Kernel.Repositories;

namespace Tracksys.Modules.Citizen.Application.Abstractions;

public interface ICitizenUnitOfWork : IUnitOfWork
{
    IRepository<Complaint, int> Complaints { get; }
    IRepository<ComplaintCategory, int> ComplaintCategories { get; }
}
