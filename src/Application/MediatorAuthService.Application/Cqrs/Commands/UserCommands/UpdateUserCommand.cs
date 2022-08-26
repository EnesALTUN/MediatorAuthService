using MediatorAuthService.Application.Wrappers;
using MediatorAuthService.Domain.Entities;
using MediatR;

namespace MediatorAuthService.Application.Cqrs.Commands.UserCommands;

public class UpdateUserCommand : IRequest<ApiResponse<User>>
{
    public User User { get; set; }

    public UpdateUserCommand(User user)
    {
        User = user;
    }
}