using MediatorAuthService.Infrastructure.Data.Context;
using MediatorAuthService.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace MediatorAuthService.Api.Extensions;

/// <summary>
/// The application automatically performs migration at each installation stage.
/// It is required to be implemented for each DbContext class.
/// </summary>
public static class AutoDatabaseMigrate
{
    /// <summary>
    /// Applies pending migrations to the database and seeds the admin user.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> instance to apply migrations for.</param>
    /// <returns>The <see cref="WebApplication"/> instance.</returns>
    public static WebApplication ApplyMigration(this WebApplication app)
    {
        using (IServiceScope serviceScope = app.Services.CreateScope())
        {
            AppDbContext db = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Database.MigrateAsync().GetAwaiter().GetResult();

            db.SeedAdminUserAsync().GetAwaiter().GetResult();
        }

        return app;
    }
}