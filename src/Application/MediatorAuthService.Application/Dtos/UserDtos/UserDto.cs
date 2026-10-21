using MediatorAuthService.Domain.Entities;
using System.Linq.Expressions;

namespace MediatorAuthService.Application.Dtos.UserDtos;

public class UserDto
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public required string Surname { get; set; }

    public required string Email { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public static Expression<Func<User, UserDto>> Projection { get; } = user => new UserDto
    {
        Id = user.Id,
        Name = user.Name,
        Surname = user.Surname,
        Email = user.Email,
        IsActive = user.IsActive,
        CreatedDate = user.CreatedDate
    };

    private static readonly Func<User, UserDto> CompiledProjection = Projection.Compile();

    public static UserDto FromEntity(User user) => CompiledProjection(user);
}
