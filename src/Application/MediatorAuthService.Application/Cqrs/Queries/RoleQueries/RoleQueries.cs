using MediatorAuthService.Application.Common.Authorization;
using MediatorAuthService.Application.Dtos.RoleDtos;
using MediatorAuthService.Application.Wrappers;
using MediatR;

namespace MediatorAuthService.Application.Cqrs.Queries.RoleQueries;

public class GetRolesQuery : IRequest<ApiResponse<List<RoleDto>>>
{
}

public class GetRoleByIdQuery(Guid id) : IRequest<ApiResponse<RoleDto>>
{
    public Guid Id { get; private set; } = id;
}

public class GetPermissionsQuery : IRequest<ApiResponse<List<PermissionDefinition>>>
{
}
