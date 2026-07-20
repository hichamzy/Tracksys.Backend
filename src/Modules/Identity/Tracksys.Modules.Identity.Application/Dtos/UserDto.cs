namespace Tracksys.Modules.Identity.Application.Dtos;

public record UserDto(
    string Id,
    string Email,
    string FullName,
    IReadOnlyList<string> Roles,
    string? Scope,
    bool IsActive,
    Guid? CityId);
