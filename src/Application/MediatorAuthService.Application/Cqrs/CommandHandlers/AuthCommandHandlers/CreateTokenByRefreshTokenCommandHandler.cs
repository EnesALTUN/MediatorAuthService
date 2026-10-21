using MediatorAuthService.Application.Common.Abstractions;
using MediatorAuthService.Application.Common.Persistence;
using MediatorAuthService.Application.Cqrs.Commands.AuthCommands;
using MediatorAuthService.Application.Dtos.AuthDtos;
using MediatorAuthService.Application.Exceptions;
using MediatorAuthService.Application.Services;
using MediatorAuthService.Application.Wrappers;
using MediatorAuthService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System.Net;

namespace MediatorAuthService.Application.Cqrs.CommandHandlers.AuthCommandHandlers;

/// <summary>
/// Exchanges a refresh token for a new token pair (rotation). No access token is required, so an expired session can be renewed.
/// - The presented token is revoked and linked to its replacement.
/// - Presenting a token that was already rotated is treated as theft: every session of the user is revoked
///   and the security stamp is rotated, which also invalidates issued access tokens.
///   Tokens revoked for other reasons (logout, password change, deactivation) are simply rejected.
/// - The token is claimed with a conditional update inside a transaction, so of two concurrent requests
///   presenting the same token only one succeeds and the other is handled as reuse.
/// </summary>
public class CreateTokenByRefreshTokenCommandHandler(
    IUnitOfWork _unitOfWork,
    ITokenService _tokenService,
    IAuthSessionService _authSessionService,
    IUserAccessService _userAccessService,
    ILogger<CreateTokenByRefreshTokenCommandHandler> _logger) : IRequestHandler<CreateTokenByRefreshTokenCommand, ApiResponse<TokenDto>>
{
    private const string InvalidTokenMessage = "The refresh token is invalid or has expired.";

    public async Task<ApiResponse<TokenDto>> Handle(CreateTokenByRefreshTokenCommand request, CancellationToken cancellationToken)
    {
        string tokenHash = _tokenService.HashRefreshToken(request.RefreshToken);

        IGenericRepository<RefreshToken> repository = _unitOfWork.GetRepository<RefreshToken>();

        RefreshToken existToken = await repository
            .Where(token => token.TokenHash == tokenHash)
            .Include(token => token.User)
            .SingleOrDefaultAsync(cancellationToken)
          ?? throw new UnauthorizedException(InvalidTokenMessage);

        DateTime now = DateTime.UtcNow;

        if (existToken.ReplacedByTokenId is not null)
        {
            await RevokeEverySessionAsync(existToken.User, cancellationToken);
            throw new UnauthorizedException(InvalidTokenMessage);
        }

        if (existToken.RevokedAt is not null || existToken.ExpiresAt <= now || !existToken.User.IsActive)
            throw new UnauthorizedException(InvalidTokenMessage);

        await using IDbContextTransaction transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        int claimed = await repository.Query()
            .Where(token => token.Id == existToken.Id && token.RevokedAt == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(token => token.RevokedAt, now), cancellationToken);

        if (claimed == 0)
        {
            // Another request rotated the same token in the meantime. Nothing was changed, so the transaction is
            // closed first and the cache is invalidated only after the revocation is committed.
            await _unitOfWork.RollBackAsync(cancellationToken);
            await RevokeEverySessionAsync(existToken.User, cancellationToken);
            throw new UnauthorizedException(InvalidTokenMessage);
        }

        (TokenDto token, RefreshToken newRefreshToken) = await _authSessionService.IssueAsync(existToken.User, cancellationToken);

        existToken.RevokedAt = now;
        existToken.ReplacedByTokenId = newRefreshToken.Id;

        await _unitOfWork.CommitAsync(cancellationToken);

        return new ApiResponse<TokenDto>
        {
            Data = token,
            IsSuccessful = true,
            StatusCode = (int)HttpStatusCode.OK,
            TotalItemCount = 1
        };
    }

    private async Task RevokeEverySessionAsync(User user, CancellationToken cancellationToken)
    {
        _logger.LogWarning("Refresh token reuse detected for user {UserId}. All sessions of the user are revoked.", user.Id);

        await _authSessionService.RevokeAllAsync(user.Id, cancellationToken);
        user.RotateSecurityStamp();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _userAccessService.Invalidate(user.Id);
    }
}
