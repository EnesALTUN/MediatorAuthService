using MediatorAuthService.Api.Extensions;
using MediatorAuthService.Application.Common.Abstractions;
using MediatorAuthService.Application.Common.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace MediatorAuthService.Api.Authorization;

internal sealed record PermissionRequirement(string Permission) : IAuthorizationRequirement;

/// <summary>
/// Builds a policy on demand for every "Permission:{name}" policy name, so permissions need no registration.
/// </summary>
internal sealed class PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : DefaultAuthorizationPolicyProvider(options)
{
    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (!policyName.StartsWith(HasPermissionAttribute.PolicyPrefix, StringComparison.Ordinal))
            return await base.GetPolicyAsync(policyName);

        string permission = policyName[HasPermissionAttribute.PolicyPrefix.Length..];

        return new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
            .RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(permission))
            .Build();
    }
}

/// <summary>
/// Checks the requirement against the current (cached) permissions of the user, not against token claims,
/// so role changes take effect without re-login.
/// </summary>
internal sealed class PermissionAuthorizationHandler(IUserAccessService userAccessService) : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        Guid? userId = context.User.GetUserId();

        if (userId is null)
            return;

        UserAccessSnapshot? access = await userAccessService.GetAsync(userId.Value, CancellationToken.None);

        if (access is { IsActive: true } && access.HasPermission(requirement.Permission))
            context.Succeed(requirement);
    }
}

internal static class PermissionAuthorizationExtension
{
    public static IServiceCollection AddPermissionAuthorization(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .Build());

        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

        return services;
    }
}
