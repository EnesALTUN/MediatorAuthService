using MediatorAuthService.Application.Common.Authorization;

namespace MediatorAuthService.Application.Common.Abstractions;

/// <summary>
/// Provides the cached authorization state (status, security stamp, roles and permissions) of users.
/// </summary>
public interface IUserAccessService
{
    /// <summary>
    /// Returns the access snapshot of the user, or null if the user does not exist.
    /// </summary>
    Task<UserAccessSnapshot?> GetAsync(Guid userId, CancellationToken cancellationToken);

    void Invalidate(Guid userId);

    /// <summary>
    /// Clears every cached snapshot. Call after role or role-permission changes.
    /// </summary>
    void InvalidateAll();
}
