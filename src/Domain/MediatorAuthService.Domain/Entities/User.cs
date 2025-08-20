using MediatorAuthService.Domain.Core.Base.Concrete;
using MediatorAuthService.Domain.Core.Base.Abstract;

namespace MediatorAuthService.Domain.Entities;

public class User : BaseEntity, IEntity
{
    public required string Name { get; set; }

    public required string Surname { get; set; }

    public required string Email { get; set; }

    public required string Password { get; set; }

    public required string RefreshToken { get; set; }
}