using MediatorAuthService.Domain.Core.Attributes;
using MediatorAuthService.Domain.Core.Base.Abstract;
using MediatorAuthService.Domain.Core.Base.Concrete;

namespace MediatorAuthService.Domain.Entities;

public class User : BaseEntity, IEntity
{
    public required string Name { get; set; }

    public required string Surname { get; set; }

    public required string Email { get; set; }

    [NotSortable]
    public required string Password { get; set; }

    [NotSortable]
    public required string RefreshToken { get; set; }
}