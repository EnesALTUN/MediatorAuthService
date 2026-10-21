using MediatorAuthService.Application.Common.Abstractions;
using MediatorAuthService.Application.Common.Authorization;
using MediatorAuthService.Application.Common.Extensions;
using MediatorAuthService.Application.Common.Persistence;
using MediatorAuthService.Application.Cqrs.Commands.UserCommands;
using MediatorAuthService.Application.Dtos.UserDtos;
using MediatorAuthService.Application.Exceptions;
using MediatorAuthService.Application.Services;
using MediatorAuthService.Application.Wrappers;
using MediatorAuthService.Domain.Entities;
using MediatR;
using System.Net;

namespace MediatorAuthService.Application.Cqrs.CommandHandlers.UserComandHandlers;

/// <summary>
/// Updates the profile of a user. Users may update themselves; updating someone else requires Users.Update
/// and permissions covering those of the target user.
/// - If the e-mail address changes, it must not belong to another user.
/// - Deactivating a user ends all of their sessions. Users cannot change their own activation status.
/// Passwords are changed through the change-password endpoint.
/// </summary>
public class UpdateUserCommandHandler(
    IUnitOfWork _unitOfWork,
    IAccessGuard _accessGuard,
    IAuthSessionService _authSessionService,
    IUserAccessService _userAccessService) : IRequestHandler<UpdateUserCommand, ApiResponse<UserDto>>
{
    public async Task<ApiResponse<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        bool isSelf = await _accessGuard.EnsureSelfOrPermissionAsync(request.Id, Permissions.Users.Update, cancellationToken);
        await _accessGuard.EnsureCanManageUserAsync(request.Id, cancellationToken);

        IGenericRepository<User> repository = _unitOfWork.GetRepository<User>();

        User existUser = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("User is not found.");

        string email = request.Email.NormalizeEmail();

        if (!string.Equals(existUser.Email, email, StringComparison.OrdinalIgnoreCase)
            && await repository.AnyAsync(user => user.Email == email && user.Id != existUser.Id, cancellationToken))
            throw new ConflictException("The entered e-mail address is used.");

        existUser.Name = request.Name.Trim();
        existUser.Surname = request.Surname.Trim();
        existUser.Email = email;

        if (request.IsActive is bool isActive && isActive != existUser.IsActive)
        {
            if (isSelf)
                throw new ForbiddenException("You cannot change the activation status of your own account.");

            existUser.IsActive = isActive;

            if (!isActive)
            {
                existUser.RotateSecurityStamp();
                await _authSessionService.RevokeAllAsync(existUser.Id, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _userAccessService.Invalidate(existUser.Id);

        return new ApiResponse<UserDto>
        {
            Data = UserDto.FromEntity(existUser),
            IsSuccessful = true,
            StatusCode = (int)HttpStatusCode.OK,
            TotalItemCount = 1
        };
    }
}
