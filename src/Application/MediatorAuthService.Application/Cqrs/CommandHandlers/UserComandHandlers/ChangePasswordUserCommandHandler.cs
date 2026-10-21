using MediatorAuthService.Application.Common.Abstractions;
using MediatorAuthService.Application.Common.Authorization;
using MediatorAuthService.Application.Common.Persistence;
using MediatorAuthService.Application.Cqrs.Commands.UserCommands;
using MediatorAuthService.Application.Dtos.ResponseDtos;
using MediatorAuthService.Application.Exceptions;
using MediatorAuthService.Application.Services;
using MediatorAuthService.Application.Wrappers;
using MediatorAuthService.Domain.Core.Extensions;
using MediatorAuthService.Domain.Entities;
using MediatR;
using System.Net;

namespace MediatorAuthService.Application.Cqrs.CommandHandlers.UserComandHandlers;

/// <summary>
/// Changes the password of the request owner after verifying the current password.
/// All sessions are ended afterwards (refresh tokens are revoked and the security stamp is rotated),
/// so the client must sign in again with the new password.
/// </summary>
public class ChangePasswordUserCommandHandler(
    IUnitOfWork _unitOfWork,
    IAccessGuard _accessGuard,
    IAuthSessionService _authSessionService,
    IUserAccessService _userAccessService) : IRequestHandler<ChangePasswordUserCommand, ApiResponse<NoData>>
{
    public async Task<ApiResponse<NoData>> Handle(ChangePasswordUserCommand request, CancellationToken cancellationToken)
    {
        Guid userId = _accessGuard.CurrentUserId;

        User existUser = await _unitOfWork.GetRepository<User>().GetByIdAsync(userId, cancellationToken)
            ?? throw new UnauthorizedException("The user account is not active.");

        if (!existUser.IsActive)
            throw new UnauthorizedException("The user account is not active.");

        if (!HashingManager.VerifyHashedValue(existUser.Password, request.OldPassword))
            throw new BusinessException("The current password is incorrect.");

        existUser.Password = HashingManager.HashValue(request.NewPassword);
        existUser.RotateSecurityStamp();
        await _authSessionService.RevokeAllAsync(existUser.Id, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _userAccessService.Invalidate(existUser.Id);

        return new ApiResponse<NoData>
        {
            IsSuccessful = true,
            StatusCode = (int)HttpStatusCode.NoContent
        };
    }
}
