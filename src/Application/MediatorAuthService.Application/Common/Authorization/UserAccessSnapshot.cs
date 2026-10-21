namespace MediatorAuthService.Application.Common.Authorization;

public sealed record UserAccessSnapshot(
    Guid UserId,
    bool IsActive,
    Guid SecurityStamp,
    bool IsSuperAdmin,
    IReadOnlyList<Guid> RoleIds,
    IReadOnlySet<string> Permissions)
{
    public bool HasPermission(string permission) => IsSuperAdmin || Permissions.Contains(permission);

    public bool HasAllPermissions(IEnumerable<string> permissions) => IsSuperAdmin || permissions.All(Permissions.Contains);
}
