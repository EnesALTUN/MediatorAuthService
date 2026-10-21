using MediatorAuthService.Application.Common.Abstractions;
using MediatorAuthService.Application.Exceptions;

namespace MediatorAuthService.Application.Common.Authorization;

internal sealed class AccessGuard(ICurrentUserService _currentUserService, IUserAccessService _userAccessService) : IAccessGuard
{
    public Guid CurrentUserId => _currentUserService.UserId ?? throw new UnauthorizedException("Authentication is required.");

    public async Task<UserAccessSnapshot> GetCurrentAccessAsync(CancellationToken cancellationToken)
    {
        UserAccessSnapshot? snapshot = await _userAccessService.GetAsync(CurrentUserId, cancellationToken);

        return snapshot is { IsActive: true }
            ? snapshot
            : throw new UnauthorizedException("The user account is not active.");
    }

    public async Task<bool> EnsureSelfOrPermissionAsync(Guid targetUserId, string permission, CancellationToken cancellationToken)
    {
        if (targetUserId == CurrentUserId)
            return true;

        UserAccessSnapshot access = await GetCurrentAccessAsync(cancellationToken);

        if (!access.HasPermission(permission))
            throw new ForbiddenException("You do not have permission to perform this action.");

        return false;
    }

    public async Task EnsureCanGrantAsync(IEnumerable<string> permissions, CancellationToken cancellationToken)
    {
        UserAccessSnapshot access = await GetCurrentAccessAsync(cancellationToken);

        List<string> missing = [.. permissions.Distinct().Where(permission => !access.HasPermission(permission))];

        if (missing.Count != 0)
            throw new ForbiddenException($"You cannot grant or revoke permissions you do not hold: {string.Join(", ", missing)}");
    }

    public async Task EnsureCanManageUserAsync(Guid targetUserId, CancellationToken cancellationToken)
    {
        if (targetUserId == CurrentUserId)
            return;

        UserAccessSnapshot actor = await GetCurrentAccessAsync(cancellationToken);
        UserAccessSnapshot? target = await _userAccessService.GetAsync(targetUserId, cancellationToken);

        if (target is null)
            return;

        if (target.IsSuperAdmin && !actor.IsSuperAdmin)
            throw new ForbiddenException("Only a SuperAdmin can manage another SuperAdmin.");

        if (!actor.HasAllPermissions(target.Permissions))
            throw new ForbiddenException("You cannot manage a user who holds permissions you do not have.");
    }
}
