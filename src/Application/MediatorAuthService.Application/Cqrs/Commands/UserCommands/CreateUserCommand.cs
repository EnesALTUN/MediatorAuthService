using MediatorAuthService.Application.Dtos.UserDtos;
using MediatorAuthService.Application.Wrappers;
using MediatR;

namespace MediatorAuthService.Application.Cqrs.Commands.UserCommands;

public class CreateUserCommand : IRequest<ApiResponse<UserDto>>
{
    public required string Name { get; set; }

    public required string Surname { get; set; }

    public required string Email { get; set; }

    public required string Password { get; set; }
}