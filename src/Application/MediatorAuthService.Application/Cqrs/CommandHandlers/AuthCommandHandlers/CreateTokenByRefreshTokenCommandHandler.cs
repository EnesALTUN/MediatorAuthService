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
using Microsoft.Extensions.Logging;
using System.Net;

namespace MediatorAuthService.Application.Cqrs.CommandHandlers.AuthCommandHandlers;

/// <summary>
/// Exchanges a refresh token for a new token pair (rotation). No access token is required, so an expired session can be renewed.
/// - The presented token is revoked and linked to its replacement.
/// - Presenting an already revoked token is treated as theft: every session of the user is revoked
///   and the security stamp is rotated, which also invalidates issued access tokens.
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

        RefreshToken existToken = await _unitOfWork.GetRepository<RefreshToken>()
            .Where(token => token.TokenHash == tokenHash)
            .Include(token => token.User)
            .SingleOrDefaultAsync(cancellationToken)
          ?? throw new UnauthorizedException(InvalidTokenMessage);

        DateTime now = DateTime.UtcNow;

        if (existToken.RevokedAt is not null)
        {
            _logger.LogWarning("Refresh token reuse detected for user {UserId}. All sessions of the user are revoked.", existToken.UserId);

            await _authSessionService.RevokeAllAsync(existToken.UserId, cancellationToken);
            existToken.User.RotateSecurityStamp();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _userAccessService.Invalidate(existToken.UserId);

            throw new UnauthorizedException(InvalidTokenMessage);
        }

        if (existToken.ExpiresAt <= now || !existToken.User.IsActive)
            throw new UnauthorizedException(InvalidTokenMessage);

        (TokenDto token, RefreshToken newRefreshToken) = await _authSessionService.IssueAsync(existToken.User, cancellationToken);

        existToken.RevokedAt = now;
        existToken.ReplacedByTokenId = newRefreshToken.Id;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ApiResponse<TokenDto>
        {
            Data = token,
            IsSuccessful = true,
            StatusCode = (int)HttpStatusCode.OK,
            TotalItemCount = 1
        };
    }
}
