using Tracksys.Modules.Tenancy.Application.Abstractions;
using Tracksys.Modules.Tenancy.Application.Dtos;
using Tracksys.Modules.Tenancy.Domain.Entities;
using Tracksys.Shared.Kernel.Results;

namespace Tracksys.Modules.Tenancy.Application.Services;

public class CityCommandService(ITenancyUnitOfWork unitOfWork)
{
    public async Task<Result<CityDto>> CreateAsync(CreateCityRequest request, CancellationToken cancellationToken = default)
    {
        bool codeTaken = await unitOfWork.Cities.AnyAsync(c => c.Code == request.Code, cancellationToken);
        if (codeTaken) return Result.Failure<CityDto>("Ce code de ville est déjà utilisé.");

        City city = City.Create(request.Name, request.Code);
        await unitOfWork.Cities.AddAsync(city, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CityDto(city.Id, city.Name, city.Code, city.IsActive);
    }

    public async Task<Result> UpdateAsync(Guid id, UpdateCityRequest request, CancellationToken cancellationToken = default)
    {
        City? city = await unitOfWork.Cities.GetByIdAsync(id, cancellationToken);
        if (city is null) return Result.Failure("Ville introuvable.");

        city.Rename(request.Name);
        city.SetActive(request.IsActive);
        unitOfWork.Cities.Update(city);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
