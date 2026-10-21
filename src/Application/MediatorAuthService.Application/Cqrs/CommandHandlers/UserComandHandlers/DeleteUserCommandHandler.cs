using MediatorAuthService.Application.Common.Abstractions;
using MediatorAuthService.Application.Common.Authorization;
using MediatorAuthService.Application.Common.Persistence;
using MediatorAuthService.Application.Cqrs.Commands.UserCommands;
using MediatorAuthService.Application.Dtos.ResponseDtos;
using MediatorAuthService.Application.Exceptions;
using MediatorAuthService.Application.Services;
using MediatorAuthService.Application.Wrappers;
using MediatorAuthService.Domain.Entities;
using MediatR;
using System.Net;

namespace MediatorAuthService.Application.Cqrs.CommandHandlers.UserComandHandlers;

/// <summary>
/// Soft-deletes the user and ends all of their sessions.
///     - Users cannot delete themselves, and may only delete users whose permissions they also hold.
///     - If the sent user ID is not found in the system, an error is returned.
/// </summary>
public class DeleteUserCommandHandler(
    IUnitOfWork _unitOfWork,
    IAccessGuard _accessGuard,
    IAuthSessionService _authSessionService,
    IUserAccessService _userAccessService) : IRequestHandler<DeleteUserCommand, ApiResponse<NoData>>
{
    public async Task<ApiResponse<NoData>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        if (request.Id == _accessGuard.CurrentUserId)
            throw new ForbiddenException("You cannot delete your own account.");

        await _accessGuard.EnsureCanManageUserAsync(request.Id, cancellationToken);

        IGenericRepository<User> repository = _unitOfWork.GetRepository<User>();

        User existUser = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("User is not found.");

        existUser.RotateSecurityStamp();
        await _authSessionService.RevokeAllAsync(existUser.Id, cancellationToken);

        repository.Remove(existUser);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _userAccessService.Invalidate(existUser.Id);

        return new ApiResponse<NoData>
        {
            StatusCode = (int)HttpStatusCode.NoContent,
            IsSuccessful = true,
        };
    }
}
