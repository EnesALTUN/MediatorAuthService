using MediatorAuthService.Api.Extensions;
using MediatorAuthService.Application.Common.Abstractions;

namespace MediatorAuthService.Api.Services;

internal sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid? UserId => httpContextAccessor.HttpContext?.User.GetUserId();
}
