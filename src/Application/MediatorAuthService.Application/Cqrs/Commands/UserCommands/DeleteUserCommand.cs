using MediatorAuthService.Application.Dtos.ResponseDtos;
using MediatorAuthService.Application.Wrappers;
using MediatR;

namespace MediatorAuthService.Application.Cqrs.Commands.UserCommands;

public class DeleteUserCommand : IRequest<ApiResponse<NoDataDto>>
{
    public Guid Id { get; private set; }

    public DeleteUserCommand(Guid id)
    {
        Id = id;
    }
}