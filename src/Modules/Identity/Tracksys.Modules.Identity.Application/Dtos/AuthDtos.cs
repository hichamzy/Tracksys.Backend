namespace Tracksys.Modules.Identity.Application.Dtos;

public record LoginRequest(string Email, string Password);

public record RefreshRequest(string RefreshToken);

public record AuthResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc,
    string UserId,
    string Email,
    string FullName,
    IReadOnlyList<string> Roles);
