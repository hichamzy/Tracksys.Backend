namespace Tracksys.Modules.Tenancy.Application.Dtos;

public record CityModulesDto(Guid CityId, IReadOnlyList<string> ModuleCodes);
