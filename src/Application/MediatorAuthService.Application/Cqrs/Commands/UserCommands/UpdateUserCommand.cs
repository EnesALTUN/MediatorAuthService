using MediatorAuthService.Application.Dtos.UserDtos;
using MediatorAuthService.Application.Wrappers;
using MediatR;

namespace MediatorAuthService.Application.Cqrs.Commands.UserCommands;

public class UpdateUserCommand : IRequest<ApiResponse<UserDto>>
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string Surname { get; set; }

    public string Email { get; set; }

    public string OldPassword { get; set; }

    public string Password { get; set; }
}