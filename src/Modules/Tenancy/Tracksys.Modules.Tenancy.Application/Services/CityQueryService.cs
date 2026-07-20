using Tracksys.Modules.Tenancy.Application.Abstractions;
using Tracksys.Modules.Tenancy.Application.Dtos;

namespace Tracksys.Modules.Tenancy.Application.Services;

public class CityQueryService(ITenancyUnitOfWork unitOfWork)
{
    public async Task<IReadOnlyList<CityDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var cities = await unitOfWork.Cities.GetAllAsync(cancellationToken);
        return cities.Select(ToDto).ToList();
    }

    public async Task<CityDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var city = await unitOfWork.Cities.GetByIdAsync(id, cancellationToken);
        return city is null ? null : ToDto(city);
    }

    private static CityDto ToDto(Domain.Entities.City city) =>
        new(city.Id, city.Name, city.Code, city.IsActive);
}
