namespace Tracksys.Modules.Identity.Application.Dtos;

public record CreateUserRequest(string Email, string FullName, string Password, string Role, string? Scope, Guid? CityId);
