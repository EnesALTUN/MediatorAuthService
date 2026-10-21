using MediatorAuthService.Application.Common.Abstractions;
using MediatorAuthService.Application.Common.Authorization;
using MediatorAuthService.Application.Common.Persistence;
using MediatorAuthService.Application.Cqrs.Commands.RoleCommands;
using MediatorAuthService.Application.Dtos.RoleDtos;
using MediatorAuthService.Application.Exceptions;
using MediatorAuthService.Application.Wrappers;
using MediatorAuthService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace MediatorAuthService.Application.Cqrs.CommandHandlers.RoleCommandHandlers;

/// <summary>
/// Renames a role and replaces its permission list. System roles cannot be modified, and the caller
/// must hold every permission being added or removed. Changes take effect immediately for all users of the role.
/// </summary>
public class UpdateRoleCommandHandler(
    IUnitOfWork _unitOfWork,
    IAccessGuard _accessGuard,
    IUserAccessService _userAccessService) : IRequestHandler<UpdateRoleCommand, ApiResponse<RoleDto>>
{
    public async Task<ApiResponse<RoleDto>> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        IGenericRepository<Role> repository = _unitOfWork.GetRepository<Role>();

        Role role = await repository
            .Where(role => role.Id == request.Id)
            .Include(role => role.Permissions)
            .SingleOrDefaultAsync(cancellationToken)
          ?? throw new NotFoundException("Role is not found.");

        if (role.IsSystem)
            throw new ForbiddenException("System roles cannot be modified.");

        HashSet<string> currentPermissions = [.. role.Permissions.Select(permission => permission.Permission)];
        HashSet<string> requestedPermissions = [.. request.Permissions];

        HashSet<string> changedPermissions = [.. currentPermissions];
        changedPermissions.SymmetricExceptWith(requestedPermissions);

        await _accessGuard.EnsureCanGrantAsync(changedPermissions, cancellationToken);

        string name = request.Name.Trim();

        if (await repository.AnyAsync(existRole => existRole.Name == name && existRole.Id != role.Id, cancellationToken))
            throw new ConflictException("A role with the same name already exists.");

        role.Name = name;
        role.Description = request.Description?.Trim();

        foreach (RolePermission removed in role.Permissions.Where(permission => !requestedPermissions.Contains(permission.Permission)).ToList())
            role.Permissions.Remove(removed);

        foreach (string added in requestedPermissions.Except(currentPermissions))
            role.Permissions.Add(new RolePermission { RoleId = role.Id, Permission = added });

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _userAccessService.InvalidateAll();

        RoleDto updatedRole = await repository.GetByIdAsync(role.Id, RoleDto.Projection, cancellationToken)
            ?? throw new NotFoundException("Role is not found.");

        return new ApiResponse<RoleDto>
        {
            Data = updatedRole.WithEffectivePermissions(),
            IsSuccessful = true,
            StatusCode = (int)HttpStatusCode.OK,
            TotalItemCount = 1
        };
    }
}
