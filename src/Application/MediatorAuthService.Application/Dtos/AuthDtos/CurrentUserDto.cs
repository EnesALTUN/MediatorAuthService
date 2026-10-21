using MediatorAuthService.Application.Dtos.RoleDtos;

namespace MediatorAuthService.Application.Dtos.AuthDtos;

/// <summary>
/// The signed-in user with effective permissions, used by clients to decide which screens and actions to show.
/// </summary>
public class CurrentUserDto
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public required string Surname { get; set; }

    public required string Email { get; set; }

    public bool IsSuperAdmin { get; set; }

    public List<RoleSummaryDto> Roles { get; set; } = [];

    public List<string> Permissions { get; set; } = [];
}
