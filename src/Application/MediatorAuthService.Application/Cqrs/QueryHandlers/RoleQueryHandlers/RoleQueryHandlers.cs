using MediatorAuthService.Application.Common.Authorization;
using MediatorAuthService.Application.Common.Persistence;
using MediatorAuthService.Application.Cqrs.Queries.RoleQueries;
using MediatorAuthService.Application.Dtos.RoleDtos;
using MediatorAuthService.Application.Exceptions;
using MediatorAuthService.Application.Wrappers;
using MediatorAuthService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace MediatorAuthService.Application.Cqrs.QueryHandlers.RoleQueryHandlers;

/// <summary>
/// Lists all roles with their permissions and user counts; system roles first.
/// </summary>
public class GetRolesQueryHandler(IUnitOfWork _unitOfWork) : IRequestHandler<GetRolesQuery, ApiResponse<List<RoleDto>>>
{
    public async Task<ApiResponse<List<RoleDto>>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        List<RoleDto> roles = await _unitOfWork.GetRepository<Role>()
            .Query()
            .OrderByDescending(role => role.IsSystem)
            .ThenBy(role => role.Name)
            .Select(RoleDto.Projection)
            .ToListAsync(cancellationToken);

        return new ApiResponse<List<RoleDto>>
        {
            Data = [.. roles.Select(role => role.WithEffectivePermissions())],
            IsSuccessful = true,
            StatusCode = (int)HttpStatusCode.OK,
            TotalItemCount = roles.Count
        };
    }
}

public class GetRoleByIdQueryHandler(IUnitOfWork _unitOfWork) : IRequestHandler<GetRoleByIdQuery, ApiResponse<RoleDto>>
{
    public async Task<ApiResponse<RoleDto>> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        RoleDto role = await _unitOfWork.GetRepository<Role>().GetByIdAsync(request.Id, RoleDto.Projection, cancellationToken)
            ?? throw new NotFoundException("Role is not found.");

        return new ApiResponse<RoleDto>
        {
            Data = role.WithEffectivePermissions(),
            IsSuccessful = true,
            StatusCode = (int)HttpStatusCode.OK,
            TotalItemCount = 1
        };
    }
}

/// <summary>
/// Returns the permission catalog so that clients can build role editing screens.
/// </summary>
public class GetPermissionsQueryHandler : IRequestHandler<GetPermissionsQuery, ApiResponse<List<PermissionDefinition>>>
{
    public Task<ApiResponse<List<PermissionDefinition>>> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new ApiResponse<List<PermissionDefinition>>
        {
            Data = [.. Permissions.All],
            IsSuccessful = true,
            StatusCode = (int)HttpStatusCode.OK,
            TotalItemCount = Permissions.All.Count
        });
    }
}
