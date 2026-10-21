using MediatorAuthService.Domain.Core.Attributes;
using MediatorAuthService.Domain.Core.Base.Concrete;

namespace MediatorAuthService.Domain.Entities;

public class Role : BaseEntity
{
    public required string Name { get; set; }

    public string? Description { get; set; }

    /// <summary>
    /// System roles are created by the application and cannot be modified or deleted.
    /// </summary>
    [NotSortable]
    public bool IsSystem { get; set; }

    [NotSortable]
    public ICollection<RolePermission> Permissions { get; set; } = [];

    [NotSortable]
    public ICollection<UserRole> UserRoles { get; set; } = [];
}
