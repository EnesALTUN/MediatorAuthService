using MediatorAuthService.Application.Dtos.RoleDtos;
using MediatorAuthService.Application.Wrappers;
using MediatR;

namespace MediatorAuthService.Application.Cqrs.Commands.RoleCommands;

public class CreateRoleCommand : IRequest<ApiResponse<RoleDto>>
{
    public required string Name { get; set; }

    public string? Description { get; set; }

    /// <summary>
    /// Permission names from the permission catalog.
    /// </summary>
    public List<string> Permissions { get; set; } = [];
}
