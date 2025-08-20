using MediatorAuthService.Domain.Core.Base.Concrete;

namespace MediatorAuthService.Application.Dtos.UserDtos;

public class UserDto : BaseDto
{
    public required string Name { get; set; }

    public required string Surname { get; set; }

    public required string Email { get; set; }

    public bool IsActive { get; set; }
}