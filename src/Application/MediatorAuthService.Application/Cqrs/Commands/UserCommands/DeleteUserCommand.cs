using MediatorAuthService.Application.Dtos.ResponseDtos;
using MediatorAuthService.Application.Wrappers;
using MediatR;

namespace MediatorAuthService.Application.Cqrs.Commands.UserCommands;

public class DeleteUserCommand(Guid id) : IRequest<ApiResponse<NoDataDto>>
{
    public Guid Id { get; private set; } = id;
}