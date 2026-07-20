using System.Security.Claims;
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
        // Au moment du login, aucun JWT n'existe encore donc ICurrentTenantAccessor.CityId
        // est null — le HasQueryFilter global sur ApplicationUser exclurait sinon TOUT
        // utilisateur rattaché à une ville, empêchant quiconque de se connecter.
        string normalizedEmail = request.Email.Trim().ToUpperInvariant();
        ApplicationUser? user = await unitOfWork.FindUserByEmailIgnoringTenantAsync(normalizedEmail, cancellationToken);
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

        // Même raison qu'en LoginAsync : pas de JWT dans une requête de refresh silencieux,
        // le HasQueryFilter par ville exclurait tout utilisateur tenanté.
        ApplicationUser? user = await unitOfWork.FindUserByIdIgnoringTenantAsync(stored.UserId, cancellationToken);
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
        // SuperAdmin (CityId null) n'emporte pas de claim city_id — absence de claim, jamais une
        // valeur sentinelle, pour que ICurrentTenantAccessor distingue clairement "toutes villes"
        // (IsSuperAdmin dérivé du rôle) d'un token cassé/sans ville (fail-closed).
        List<Claim> extraClaims = [];
        if (user.CityId is Guid cityId)
            extraClaims.Add(new Claim("city_id", cityId.ToString()));

        GeneratedAccessToken access = tokenGenerator.GenerateAccessToken(user.Id, user.Email!, roles, extraClaims);
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
            roles.ToList(),
            user.CityId));
    }
}
