using MediatorAuthService.Domain.Core.Base.Concrete;

namespace MediatorAuthService.Application.Dtos.UserDtos;

public class UserDto : BaseDto
{
    public string Name { get; set; }

    public string Surname { get; set; }

    public string Email { get; set; }

    public bool IsActive { get; set; }
}