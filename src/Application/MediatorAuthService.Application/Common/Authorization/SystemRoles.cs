namespace MediatorAuthService.Application.Common.Authorization;

public static class SystemRoles
{
    /// <summary>
    /// Implicitly holds every permission. Seeded by migration.
    /// </summary>
    public static readonly Guid SuperAdminId = Guid.Parse("8f3c2a0e-5b7d-4c1a-9e6f-2d4b8a1c3e5f");

    public const string SuperAdminName = "SuperAdmin";
}
