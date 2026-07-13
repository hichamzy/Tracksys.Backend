using System.Security.Claims;

namespace Tracksys.Modules.Identity.Application.Abstractions;

public record GeneratedAccessToken(string Token, DateTime ExpiresAtUtc);

public interface IJwtTokenGenerator
{
    GeneratedAccessToken GenerateAccessToken(string userId, string email, IEnumerable<string> roles, IEnumerable<Claim>? extraClaims = null);

    string GenerateRefreshToken();

    string HashRefreshToken(string refreshToken);
}
