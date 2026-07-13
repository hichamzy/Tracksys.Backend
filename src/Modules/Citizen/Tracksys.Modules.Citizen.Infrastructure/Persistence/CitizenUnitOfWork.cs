using Tracksys.Modules.Citizen.Application.Abstractions;
using Tracksys.Modules.Citizen.Domain.Entities;
using Tracksys.Shared.Infrastructure.Persistence;
using Tracksys.Shared.Kernel.Repositories;

namespace Tracksys.Modules.Citizen.Infrastructure.Persistence;

public class CitizenUnitOfWork : UnitOfWork<CitizenDbContext>, ICitizenUnitOfWork
{
    public IRepository<Complaint, int> Complaints { get; }
    public IRepository<ComplaintCategory, int> ComplaintCategories { get; }

    public CitizenUnitOfWork(CitizenDbContext dbContext) : base(dbContext)
    {
        Complaints = new Repository<Complaint, int>(dbContext);
        ComplaintCategories = new Repository<ComplaintCategory, int>(dbContext);
    }
}
