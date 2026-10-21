using MediatorAuthService.Application.Common.Authorization;
using MediatorAuthService.Application.Common.Extensions;
using MediatorAuthService.Domain.Core.Extensions;
using MediatorAuthService.Domain.Entities;
using MediatorAuthService.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MediatorAuthService.Infrastructure.Data.Seed;

public class SeedOptions
{
    public const string SectionName = "Seed";

    public string? AdminEmail { get; set; }

    public string? AdminPassword { get; set; }
}

public static class DatabaseInitializer
{
    // Credentials that earlier versions seeded unconditionally. They are public through the repository history.
    private const string LegacyAdminEmail = "admin@gmail.com";
    private const string LegacyAdminPassword = "qwe123ASD!";

    /// <summary>
    /// Disables the administrator account that earlier versions created with hard-coded credentials, as long as
    /// its password was never changed. Its sessions are ended and the security stamp is rotated.
    /// </summary>
    public static async Task DisableLegacyDefaultAdminAsync(this AppDbContext context, ILogger logger, CancellationToken cancellationToken = default)
    {
        User? legacyAdmin = await context.Users
            .Include(user => user.RefreshTokens)
            .SingleOrDefaultAsync(user => user.Email == LegacyAdminEmail, cancellationToken);

        if (legacyAdmin is not { IsActive: true } || !HasLegacyPassword(legacyAdmin))
            return;

        DateTime now = DateTime.UtcNow;

        legacyAdmin.IsActive = false;
        legacyAdmin.RotateSecurityStamp();

        foreach (RefreshToken token in legacyAdmin.RefreshTokens.Where(token => token.IsActive(now)))
            token.RevokedAt = now;

        await context.SaveChangesAsync(cancellationToken);

        logger.LogWarning("The legacy default administrator {Email} still used its publicly known password and has been disabled.", LegacyAdminEmail);
    }

    /// <summary>
    /// Bootstraps the first SuperAdmin from configuration (Seed:AdminEmail / Seed:AdminPassword).
    /// Runs only while no SuperAdmin exists. If the configured user already exists, the role is assigned
    /// without touching the password; otherwise the user is created with the configured password.
    /// The legacy default administrator is the exception: it is promoted only when Seed:AdminPassword is set,
    /// and its publicly known password is replaced with it.
    /// </summary>
    public static async Task SeedSuperAdminAsync(this AppDbContext context, SeedOptions options, ILogger logger, CancellationToken cancellationToken = default)
    {
        if (await context.UserRoles.AnyAsync(userRole => userRole.RoleId == SystemRoles.SuperAdminId, cancellationToken))
            return;

        if (string.IsNullOrWhiteSpace(options.AdminEmail))
        {
            logger.LogWarning("No SuperAdmin exists. Set Seed:AdminEmail (and Seed:AdminPassword for a new user) to bootstrap one.");
            return;
        }

        string email = options.AdminEmail.NormalizeEmail();

        User? adminUser = await context.Users.SingleOrDefaultAsync(user => user.Email == email, cancellationToken);

        if (adminUser is null)
        {
            if (string.IsNullOrWhiteSpace(options.AdminPassword))
            {
                logger.LogWarning("No SuperAdmin exists and Seed:AdminPassword is not set, so the user {Email} cannot be created.", email);
                return;
            }

            adminUser = new User
            {
                Email = email,
                Password = HashingManager.HashValue(options.AdminPassword),
                Name = "Admin",
                Surname = "Admin"
            };
            context.Users.Add(adminUser);
        }
        else if (HasLegacyPassword(adminUser))
        {
            if (string.IsNullOrWhiteSpace(options.AdminPassword))
            {
                logger.LogWarning("The user {Email} still uses the legacy default password. Set Seed:AdminPassword to replace it before it can become SuperAdmin.", email);
                return;
            }

            adminUser.Password = HashingManager.HashValue(options.AdminPassword);
            adminUser.IsActive = true;
            adminUser.RotateSecurityStamp();
        }

        adminUser.UserRoles.Add(new UserRole { RoleId = SystemRoles.SuperAdminId });

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("SuperAdmin role assigned to {Email}.", email);
    }

    private static bool HasLegacyPassword(User user) =>
        user.Email == LegacyAdminEmail && HashingManager.VerifyHashedValue(user.Password, LegacyAdminPassword);
}
