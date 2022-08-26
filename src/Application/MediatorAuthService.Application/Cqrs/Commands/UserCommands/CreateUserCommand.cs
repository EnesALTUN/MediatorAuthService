using MediatorAuthService.Application.Wrappers;
using MediatorAuthService.Domain.Entities;
using MediatR;

namespace MediatorAuthService.Application.Cqrs.Commands.UserCommands;

public class CreateUserCommand : IRequest<ApiResponse<User>>
{
    public User User { get; set; }

    public CreateUserCommand(User user)
    {
        User = user;
    }
}