using Microsoft.AspNetCore.Identity;
using Tracksys.Modules.Identity.Application.Abstractions;
using Tracksys.Modules.Identity.Application.Dtos;
using Tracksys.Modules.Identity.Application.Options;
using Tracksys.Modules.Identity.Domain.Entities;
using Tracksys.Shared.Kernel.Results;
using Microsoft.Extensions.Options;

namespace Tracksys.Modules.Identity.Application.Services;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    IJwtTokenGenerator tokenGenerator,
    IIdentityUnitOfWork unitOfWork,
    IOptions<JwtOptions> jwtOptions)
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, string? ipAddress, CancellationToken cancellationToken = default)
    {
        ApplicationUser? user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || !user.IsActive)
            return Result.Failure<AuthResponse>("Identifiants invalides.");

        bool passwordValid = await userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
            return Result.Failure<AuthResponse>("Identifiants invalides.");

        IList<string> roles = await userManager.GetRolesAsync(user);

        return await IssueTokensAsync(user, roles, ipAddress, cancellationToken);
    }

    public async Task<Result<AuthResponse>> RefreshAsync(RefreshRequest request, string? ipAddress, CancellationToken cancellationToken = default)
    {
        string incomingHash = tokenGenerator.HashRefreshToken(request.RefreshToken);

        RefreshToken? stored = (await unitOfWork.RefreshTokens
                .FindAsync(rt => rt.TokenHash == incomingHash, cancellationToken))
            .SingleOrDefault();

        if (stored is null || !stored.IsActive)
            return Result.Failure<AuthResponse>("Refresh token invalide ou expiré.");

        ApplicationUser? user = await userManager.FindByIdAsync(stored.UserId);
        if (user is null || !user.IsActive)
            return Result.Failure<AuthResponse>("Utilisateur introuvable ou inactif.");

        IList<string> roles = await userManager.GetRolesAsync(user);

        stored.Revoke();
        unitOfWork.RefreshTokens.Update(stored);

        return await IssueTokensAsync(user, roles, ipAddress, cancellationToken);
    }

    public async Task RevokeAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        string hash = tokenGenerator.HashRefreshToken(refreshToken);
        RefreshToken? stored = (await unitOfWork.RefreshTokens
                .FindAsync(rt => rt.TokenHash == hash, cancellationToken))
            .SingleOrDefault();

        if (stored is null || !stored.IsActive) return;

        stored.Revoke();
        unitOfWork.RefreshTokens.Update(stored);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Result<AuthResponse>> IssueTokensAsync(
        ApplicationUser user, IList<string> roles, string? ipAddress, CancellationToken cancellationToken)
    {
        GeneratedAccessToken access = tokenGenerator.GenerateAccessToken(user.Id, user.Email!, roles);
        string refreshPlain = tokenGenerator.GenerateRefreshToken();
        string refreshHash = tokenGenerator.HashRefreshToken(refreshPlain);
        DateTime refreshExpiry = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays);

        RefreshToken entity = RefreshToken.Create(user.Id, refreshHash, refreshExpiry, ipAddress);
        await unitOfWork.RefreshTokens.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new AuthResponse(
            access.Token,
            access.ExpiresAtUtc,
            refreshPlain,
            refreshExpiry,
            user.Id,
            user.Email!,
            user.FullName,
            roles.ToList()));
    }
}
