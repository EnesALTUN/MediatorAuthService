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
    /// <summary>
    /// Bootstraps the first SuperAdmin from configuration (Seed:AdminEmail / Seed:AdminPassword).
    /// Runs only while no SuperAdmin exists. If the configured user already exists, the role is assigned
    /// without touching the password; otherwise the user is created with the configured password.
    /// No credentials are hard-coded.
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

        adminUser.UserRoles.Add(new UserRole { RoleId = SystemRoles.SuperAdminId });

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("SuperAdmin role assigned to {Email}.", email);
    }
}
