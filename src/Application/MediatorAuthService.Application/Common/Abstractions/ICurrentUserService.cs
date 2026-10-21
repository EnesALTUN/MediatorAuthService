namespace MediatorAuthService.Application.Common.Abstractions;

public interface ICurrentUserService
{
    /// <summary>
    /// ID of the authenticated user making the request; null for anonymous requests and background work.
    /// </summary>
    Guid? UserId { get; }
}
