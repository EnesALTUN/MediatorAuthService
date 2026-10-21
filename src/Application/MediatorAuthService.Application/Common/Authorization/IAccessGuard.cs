namespace MediatorAuthService.Application.Common.Authorization;

/// <summary>
/// Resource-level authorization checks that cannot be expressed as a plain endpoint policy.
/// </summary>
public interface IAccessGuard
{
    Guid CurrentUserId { get; }

    Task<UserAccessSnapshot> GetCurrentAccessAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Succeeds when the target is the current user or the current user holds the permission. Returns true for self.
    /// </summary>
    Task<bool> EnsureSelfOrPermissionAsync(Guid targetUserId, string permission, CancellationToken cancellationToken);

    /// <summary>
    /// Prevents privilege escalation: the current user may only grant or revoke permissions they hold.
    /// </summary>
    Task EnsureCanGrantAsync(IEnumerable<string> permissions, CancellationToken cancellationToken);

    /// <summary>
    /// The current user may only manage users whose permissions are a subset of their own.
    /// </summary>
    Task EnsureCanManageUserAsync(Guid targetUserId, CancellationToken cancellationToken);
}
