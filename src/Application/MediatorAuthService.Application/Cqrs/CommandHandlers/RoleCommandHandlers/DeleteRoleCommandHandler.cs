using MediatorAuthService.Application.Common.Abstractions;
using MediatorAuthService.Application.Common.Authorization;
using MediatorAuthService.Application.Common.Persistence;
using MediatorAuthService.Application.Cqrs.Commands.RoleCommands;
using MediatorAuthService.Application.Dtos.ResponseDtos;
using MediatorAuthService.Application.Exceptions;
using MediatorAuthService.Application.Wrappers;
using MediatorAuthService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace MediatorAuthService.Application.Cqrs.CommandHandlers.RoleCommandHandlers;

/// <summary>
/// Soft-deletes a role and removes it from every user. System roles cannot be deleted, and the caller
/// must hold every permission of the role.
/// </summary>
public class DeleteRoleCommandHandler(
    IUnitOfWork _unitOfWork,
    IAccessGuard _accessGuard,
    IUserAccessService _userAccessService) : IRequestHandler<DeleteRoleCommand, ApiResponse<NoData>>
{
    public async Task<ApiResponse<NoData>> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        IGenericRepository<Role> repository = _unitOfWork.GetRepository<Role>();

        Role role = await repository
            .Where(role => role.Id == request.Id)
            .Include(role => role.Permissions)
            .Include(role => role.UserRoles)
            .SingleOrDefaultAsync(cancellationToken)
          ?? throw new NotFoundException("Role is not found.");

        if (role.IsSystem)
            throw new ForbiddenException("System roles cannot be deleted.");

        await _accessGuard.EnsureCanGrantAsync(role.Permissions.Select(permission => permission.Permission), cancellationToken);

        _unitOfWork.GetRepository<UserRole>().RemoveRange(role.UserRoles);
        _unitOfWork.GetRepository<RolePermission>().RemoveRange(role.Permissions);
        repository.Remove(role);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _userAccessService.InvalidateAll();

        return new ApiResponse<NoData>
        {
            IsSuccessful = true,
            StatusCode = (int)HttpStatusCode.NoContent
        };
    }
}
