namespace MediatorAuthService.Domain.Entities;

public class RolePermission
{
    public Guid RoleId { get; set; }

    public required string Permission { get; set; }

    public Role Role { get; set; } = null!;
}
