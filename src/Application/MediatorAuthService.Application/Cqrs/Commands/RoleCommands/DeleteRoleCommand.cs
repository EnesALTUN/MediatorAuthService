using MediatorAuthService.Application.Dtos.ResponseDtos;
using MediatorAuthService.Application.Wrappers;
using MediatR;

namespace MediatorAuthService.Application.Cqrs.Commands.RoleCommands;

public class DeleteRoleCommand(Guid id) : IRequest<ApiResponse<NoData>>
{
    public Guid Id { get; private set; } = id;
}
