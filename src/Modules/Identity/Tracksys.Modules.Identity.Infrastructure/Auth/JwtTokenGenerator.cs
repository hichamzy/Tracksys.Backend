using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Tracksys.Modules.Identity.Application.Abstractions;
using Tracksys.Modules.Identity.Application.Options;

namespace Tracksys.Modules.Identity.Infrastructure.Auth;

public class JwtTokenGenerator(IOptions<JwtOptions> options) : IJwtTokenGenerator
{
    private readonly JwtOptions _options = options.Value;

    public GeneratedAccessToken GenerateAccessToken(
        string userId, string email, IEnumerable<string> roles, IEnumerable<Claim>? extraClaims = null)
    {
        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            ..roles.Select(r => new Claim(ClaimTypes.Role, r)),
        ];

        if (extraClaims is not null) claims.AddRange(extraClaims);

        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(_options.SigningKey));
        SigningCredentials creds = new(key, SecurityAlgorithms.HmacSha256);
        DateTime expires = DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes);

        JwtSecurityToken token = new(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return new GeneratedAccessToken(new JwtSecurityTokenHandler().WriteToken(token), expires);
    }

    public string GenerateRefreshToken()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    public string HashRefreshToken(string refreshToken)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToHexString(hash);
    }
}
