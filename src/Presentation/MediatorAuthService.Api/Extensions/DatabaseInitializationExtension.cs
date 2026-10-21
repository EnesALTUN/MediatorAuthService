using MediatorAuthService.Infrastructure.Data.Context;
using MediatorAuthService.Infrastructure.Data.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MediatorAuthService.Api.Extensions;

/// <summary>
/// Prepares the database at startup according to Database:InitializationMode and seeds the first SuperAdmin.
/// </summary>
public static class DatabaseInitializationExtension
{
    private const int MaxMigrationAttempts = 5;

    /// <summary>
    /// Migrate (default): applies pending migrations, retrying while the database server starts up.
    /// EnsureCreated: creates the schema without migrations (used by tests).
    /// None: leaves the schema untouched.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> instance.</param>
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        string mode = app.Configuration["Database:InitializationMode"] ?? "Migrate";

        if (string.Equals(mode, "None", StringComparison.OrdinalIgnoreCase))
            return;

        await using AsyncServiceScope serviceScope = app.Services.CreateAsyncScope();

        AppDbContext db = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
        ILogger logger = serviceScope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DatabaseInitializationExtension));

        if (string.Equals(mode, "EnsureCreated", StringComparison.OrdinalIgnoreCase))
            await db.Database.EnsureCreatedAsync();
        else
            await MigrateWithRetryAsync(db, logger);

        SeedOptions seedOptions = serviceScope.ServiceProvider.GetRequiredService<IOptions<SeedOptions>>().Value;

        await db.SeedSuperAdminAsync(seedOptions, logger);
    }

    private static async Task MigrateWithRetryAsync(AppDbContext db, ILogger logger)
    {
        for (int attempt = 1; ; attempt++)
        {
            try
            {
                await db.Database.MigrateAsync();
                return;
            }
            catch (Exception ex) when (attempt < MaxMigrationAttempts)
            {
                TimeSpan delay = TimeSpan.FromSeconds(attempt * 5);
                logger.LogWarning(ex, "Database migration attempt {Attempt} failed. Retrying in {Delay}.", attempt, delay);
                await Task.Delay(delay);
            }
        }
    }
}
