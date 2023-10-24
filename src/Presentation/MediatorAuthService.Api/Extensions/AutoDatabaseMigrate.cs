using MediatorAuthService.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace MediatorAuthService.Api.Extensions;

/// <summary>
/// The application automatically performs migration at each installation stage.
/// It is required to be implemented for each DbContext class.
/// </summary>
public static class AutoDatabaseMigrate
{
    public static WebApplication ApplyMigration(this WebApplication app)
    {
        using (IServiceScope serviceScope = app.Services.CreateScope())
        {
            AppDbContext db = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Database.MigrateAsync().GetAwaiter().GetResult();
        }

        return app;
    }
}