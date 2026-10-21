using Microsoft.AspNetCore.Authorization;

namespace MediatorAuthService.Api.Authorization;

/// <summary>
/// Requires the authenticated user to hold the given permission (directly through a role, or as SuperAdmin).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    internal const string PolicyPrefix = "Permission:";

    /// <summary>
    /// Creates the attribute for a permission from the permission catalog.
    /// </summary>
    /// <param name="permission">Permission name, e.g. Users.Read</param>
    public HasPermissionAttribute(string permission) : base(PolicyPrefix + permission)
    {
    }
}
