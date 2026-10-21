using MediatorAuthService.Application.Common.Abstractions;
using MediatorAuthService.Application.Common.Authorization;
using MediatorAuthService.Application.Common.Persistence;
using MediatorAuthService.Application.Cqrs.Commands.UserCommands;
using MediatorAuthService.Application.Dtos.RoleDtos;
using MediatorAuthService.Application.Exceptions;
using MediatorAuthService.Application.Wrappers;
using MediatorAuthService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace MediatorAuthService.Application.Cqrs.CommandHandlers.UserComandHandlers;

/// <summary>
/// Replaces the roles of a user. To prevent privilege escalation:
/// - the caller must hold every permission of each role being added or removed,
/// - only a SuperAdmin can assign or remove the SuperAdmin role,
/// - the last SuperAdmin cannot lose the role.
/// </summary>
public class SetUserRolesCommandHandler(
    IUnitOfWork _unitOfWork,
    IAccessGuard _accessGuard,
    IUserAccessService _userAccessService) : IRequestHandler<SetUserRolesCommand, ApiResponse<List<RoleSummaryDto>>>
{
    public async Task<ApiResponse<List<RoleSummaryDto>>> Handle(SetUserRolesCommand request, CancellationToken cancellationToken)
    {
        UserAccessSnapshot actor = await _accessGuard.GetCurrentAccessAsync(cancellationToken);
        await _accessGuard.EnsureCanManageUserAsync(request.UserId, cancellationToken);

        if (!await _unitOfWork.GetRepository<User>().AnyAsync(user => user.Id == request.UserId, cancellationToken))
            throw new NotFoundException("User is not found.");

        HashSet<Guid> requestedRoleIds = [.. request.RoleIds];

        List<Role> requestedRoles = await _unitOfWork.GetRepository<Role>()
            .Where(role => request.RoleIds.Contains(role.Id))
            .Include(role => role.Permissions)
            .ToListAsync(cancellationToken);

        if (requestedRoles.Count != requestedRoleIds.Count)
            throw new NotFoundException("One or more roles were not found.");

        IGenericRepository<UserRole> userRoleRepository = _unitOfWork.GetRepository<UserRole>();

        List<UserRole> currentUserRoles = await userRoleRepository
            .Where(userRole => userRole.UserId == request.UserId)
            .Include(userRole => userRole.Role)
                .ThenInclude(role => role.Permissions)
            .ToListAsync(cancellationToken);

        List<Role> addedRoles = [.. requestedRoles.Where(role => currentUserRoles.All(userRole => userRole.RoleId != role.Id))];
        List<UserRole> removedUserRoles = [.. currentUserRoles.Where(userRole => !requestedRoleIds.Contains(userRole.RoleId))];
        List<Role> changedRoles = [.. addedRoles, .. removedUserRoles.Select(userRole => userRole.Role)];

        if (!actor.IsSuperAdmin && changedRoles.Any(role => role.Id == SystemRoles.SuperAdminId))
            throw new ForbiddenException("Only a SuperAdmin can assign or remove the SuperAdmin role.");

        await _accessGuard.EnsureCanGrantAsync(changedRoles.SelectMany(role => role.Permissions.Select(permission => permission.Permission)), cancellationToken);

        if (removedUserRoles.Any(userRole => userRole.RoleId == SystemRoles.SuperAdminId))
        {
            // Only an active SuperAdmin can recover the system, so deactivated or deleted ones do not count.
            bool activeSuperAdminRemains = await userRoleRepository
                .Where(userRole => userRole.RoleId == SystemRoles.SuperAdminId
                    && userRole.UserId != request.UserId
                    && userRole.User.IsActive
                    && userRole.User.DeletedDate == null)
                .AnyAsync(cancellationToken);

            if (!activeSuperAdminRemains)
                throw new BusinessException("At least one active SuperAdmin must remain.");
        }

        userRoleRepository.RemoveRange(removedUserRoles);

        foreach (Role role in addedRoles)
            await userRoleRepository.AddAsync(new UserRole { UserId = request.UserId, RoleId = role.Id }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _userAccessService.Invalidate(request.UserId);

        return new ApiResponse<List<RoleSummaryDto>>
        {
            Data = [.. requestedRoles.OrderBy(role => role.Name).Select(role => new RoleSummaryDto { Id = role.Id, Name = role.Name })],
            IsSuccessful = true,
            StatusCode = (int)HttpStatusCode.OK,
            TotalItemCount = requestedRoles.Count
        };
    }
}
