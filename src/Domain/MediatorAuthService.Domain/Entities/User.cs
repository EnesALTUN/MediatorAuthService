using MediatorAuthService.Domain.Core.Attributes;
using MediatorAuthService.Domain.Core.Base.Concrete;

namespace MediatorAuthService.Domain.Entities;

public class User : BaseEntity
{
    public required string Name { get; set; }

    public required string Surname { get; set; }

    public required string Email { get; set; }

    [NotSortable]
    public required string Password { get; set; }

    /// <summary>
    /// Changes whenever the user's credentials or status change. Access tokens carrying an old stamp are rejected.
    /// </summary>
    [NotSortable]
    public Guid SecurityStamp { get; set; } = Guid.NewGuid();

    [NotSortable]
    public int AccessFailedCount { get; set; }

    [NotSortable]
    public DateTime? LockoutEnd { get; set; }

    [NotSortable]
    public ICollection<UserRole> UserRoles { get; set; } = [];

    [NotSortable]
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];

    public bool IsLockedOut(DateTime utcNow) => LockoutEnd is not null && LockoutEnd > utcNow;

    public void RotateSecurityStamp() => SecurityStamp = Guid.NewGuid();
}
