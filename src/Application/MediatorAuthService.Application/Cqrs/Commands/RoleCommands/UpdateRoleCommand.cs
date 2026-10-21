using MediatorAuthService.Application.Dtos.RoleDtos;
using MediatorAuthService.Application.Wrappers;
using MediatR;
using System.Text.Json.Serialization;

namespace MediatorAuthService.Application.Cqrs.Commands.RoleCommands;

public class UpdateRoleCommand : IRequest<ApiResponse<RoleDto>>
{
    /// <summary>
    /// Bound from the route.
    /// </summary>
    [JsonIgnore]
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    /// <summary>
    /// The complete permission list of the role; permissions not in the list are removed.
    /// </summary>
    public List<string> Permissions { get; set; } = [];
}
