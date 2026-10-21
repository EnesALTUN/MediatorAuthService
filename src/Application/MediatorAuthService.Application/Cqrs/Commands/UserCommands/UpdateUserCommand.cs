using MediatorAuthService.Application.Common.Security;
using MediatorAuthService.Application.Dtos.UserDtos;
using MediatorAuthService.Application.Wrappers;
using MediatR;
using System.Text.Json.Serialization;

namespace MediatorAuthService.Application.Cqrs.Commands.UserCommands;

public class UpdateUserCommand : IRequest<ApiResponse<UserDto>>
{
    /// <summary>
    /// Bound from the route.
    /// </summary>
    [JsonIgnore]
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public required string Surname { get; set; }

    [PersonalData]
    public required string Email { get; set; }

    /// <summary>
    /// Activates or deactivates the user. Requires the Users.Update permission and cannot be applied to your own account.
    /// </summary>
    public bool? IsActive { get; set; }
}
