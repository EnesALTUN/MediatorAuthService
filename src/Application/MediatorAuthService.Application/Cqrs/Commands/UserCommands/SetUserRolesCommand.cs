using MediatorAuthService.Application.Dtos.RoleDtos;
using MediatorAuthService.Application.Wrappers;
using MediatR;
using System.Text.Json.Serialization;

namespace MediatorAuthService.Application.Cqrs.Commands.UserCommands;

/// <summary>
/// Replaces the role assignments of a user with the given list.
/// </summary>
public class SetUserRolesCommand : IRequest<ApiResponse<List<RoleSummaryDto>>>
{
    /// <summary>
    /// Bound from the route.
    /// </summary>
    [JsonIgnore]
    public Guid UserId { get; set; }

    public List<Guid> RoleIds { get; set; } = [];
}
