using MediatorAuthService.Application.Common.Authorization;
using MediatorAuthService.Domain.Entities;
using System.Linq.Expressions;

namespace MediatorAuthService.Application.Dtos.RoleDtos;

public class RoleDto
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public bool IsSystem { get; set; }

    public List<string> Permissions { get; set; } = [];

    public int UserCount { get; set; }

    public static Expression<Func<Role, RoleDto>> Projection { get; } = role => new RoleDto
    {
        Id = role.Id,
        Name = role.Name,
        Description = role.Description,
        IsSystem = role.IsSystem,
        Permissions = role.Permissions.Select(permission => permission.Permission).ToList(),
        UserCount = role.UserRoles.Count
    };

    /// <summary>
    /// SuperAdmin holds every permission implicitly, so its stored permission list is replaced with the full catalog.
    /// </summary>
    public RoleDto WithEffectivePermissions()
    {
        Permissions = Id == SystemRoles.SuperAdminId
            ? [.. Common.Authorization.Permissions.AllNames.Order()]
            : [.. Permissions.Order()];

        return this;
    }
}

public class RoleSummaryDto
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public static Expression<Func<Role, RoleSummaryDto>> Projection { get; } = role => new RoleSummaryDto
    {
        Id = role.Id,
        Name = role.Name
    };
}
