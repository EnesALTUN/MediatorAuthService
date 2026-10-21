using MediatorAuthService.Application.Common.Abstractions;
using MediatorAuthService.Application.Common.Persistence;
using MediatorAuthService.Application.Dtos.AuthDtos;
using MediatorAuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MediatorAuthService.Application.Services;

public interface IAuthSessionService
{
    /// <summary>
    /// Creates an access token and a new refresh token for the user. The refresh token is added to the
    /// unit of work; the caller is responsible for saving changes.
    /// </summary>
    Task<(TokenDto Token, RefreshToken RefreshToken)> IssueAsync(User user, CancellationToken cancellationToken);

    /// <summary>
    /// Revokes every active refresh token of the user. The caller is responsible for saving changes.
    /// </summary>
    Task RevokeAllAsync(Guid userId, CancellationToken cancellationToken);
}

internal sealed class AuthSessionService(IUnitOfWork _unitOfWork, ITokenService _tokenService) : IAuthSessionService
{
    public async Task<(TokenDto Token, RefreshToken RefreshToken)> IssueAsync(User user, CancellationToken cancellationToken)
    {
        DateTime now = DateTime.UtcNow;

        IGenericRepository<RefreshToken> repository = _unitOfWork.GetRepository<RefreshToken>();

        // Expired tokens are no longer useful for reuse detection, so they are cleaned up on every issue.
        List<RefreshToken> expiredTokens = await repository
            .Where(token => token.UserId == user.Id && token.ExpiresAt <= now)
            .ToListAsync(cancellationToken);
        repository.RemoveRange(expiredTokens);

        AccessToken accessToken = _tokenService.CreateAccessToken(user);
        GeneratedRefreshToken generatedRefreshToken = _tokenService.CreateRefreshToken();

        RefreshToken refreshToken = new()
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = generatedRefreshToken.TokenHash,
            CreatedAt = now,
            ExpiresAt = generatedRefreshToken.ExpiresAt
        };

        await repository.AddAsync(refreshToken, cancellationToken);

        TokenDto token = new()
        {
            AccessToken = accessToken.Token,
            AccessTokenExpire = accessToken.ExpiresAt,
            RefreshToken = generatedRefreshToken.Token,
            RefreshTokenExpire = generatedRefreshToken.ExpiresAt
        };

        return (token, refreshToken);
    }

    public async Task RevokeAllAsync(Guid userId, CancellationToken cancellationToken)
    {
        DateTime now = DateTime.UtcNow;

        List<RefreshToken> activeTokens = await _unitOfWork.GetRepository<RefreshToken>()
            .Where(token => token.UserId == userId && token.RevokedAt == null && token.ExpiresAt > now)
            .ToListAsync(cancellationToken);

        foreach (RefreshToken token in activeTokens)
            token.RevokedAt = now;
    }
}
