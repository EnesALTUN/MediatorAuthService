using MediatorAuthService.Application.Common.Authorization;
using System.Security.Claims;

namespace MediatorAuthService.Api.Extensions;

internal static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal claimsPrincipal) =>
        Guid.TryParse(claimsPrincipal.FindFirstValue(CustomClaimTypes.Subject), out Guid userId) ? userId : null;

    public static string? GetSecurityStamp(this ClaimsPrincipal claimsPrincipal) =>
        claimsPrincipal.FindFirstValue(CustomClaimTypes.SecurityStamp);
}
