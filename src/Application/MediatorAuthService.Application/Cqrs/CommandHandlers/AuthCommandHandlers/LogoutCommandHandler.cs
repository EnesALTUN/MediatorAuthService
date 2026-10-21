using MediatorAuthService.Application.Common.Abstractions;
using MediatorAuthService.Application.Common.Persistence;
using MediatorAuthService.Application.Cqrs.Commands.AuthCommands;
using MediatorAuthService.Application.Dtos.ResponseDtos;
using MediatorAuthService.Application.Wrappers;
using MediatorAuthService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace MediatorAuthService.Application.Cqrs.CommandHandlers.AuthCommandHandlers;

/// <summary>
/// Revokes the given refresh token. Always succeeds so that the response does not reveal whether the token existed.
/// </summary>
public class LogoutCommandHandler(IUnitOfWork _unitOfWork, ITokenService _tokenService) : IRequestHandler<LogoutCommand, ApiResponse<NoData>>
{
    public async Task<ApiResponse<NoData>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        string tokenHash = _tokenService.HashRefreshToken(request.RefreshToken);
        DateTime now = DateTime.UtcNow;

        RefreshToken? existToken = await _unitOfWork.GetRepository<RefreshToken>()
            .Where(token => token.TokenHash == tokenHash)
            .SingleOrDefaultAsync(cancellationToken);

        if (existToken is not null && existToken.IsActive(now))
        {
            existToken.RevokedAt = now;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return new ApiResponse<NoData>
        {
            IsSuccessful = true,
            StatusCode = (int)HttpStatusCode.NoContent
        };
    }
}
