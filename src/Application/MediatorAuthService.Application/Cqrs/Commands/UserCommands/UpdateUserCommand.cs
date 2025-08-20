using MediatorAuthService.Application.Dtos.UserDtos;
using MediatorAuthService.Application.Wrappers;
using MediatR;

namespace MediatorAuthService.Application.Cqrs.Commands.UserCommands;

public class UpdateUserCommand : IRequest<ApiResponse<UserDto>>
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public required string Surname { get; set; }

    public required string Email { get; set; }

    public string? OldPassword { get; set; }

    public string? Password { get; set; }
}