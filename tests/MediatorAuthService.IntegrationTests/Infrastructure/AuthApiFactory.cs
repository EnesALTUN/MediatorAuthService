using MediatorAuthService.Infrastructure.Data.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MediatorAuthService.IntegrationTests.Infrastructure;

/// <summary>
/// Hosts the real application against an in-memory SQLite database. The schema is created with EnsureCreated
/// (migrations are SQL Server specific) and the first SuperAdmin is seeded from configuration.
/// </summary>
public class AuthApiFactory : WebApplicationFactory<Program>
{
    public const string AdminEmail = "admin@test.local";
    public const string AdminPassword = "Adm1nPassw0rd!";

    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected virtual int AuthPermitLimit => 10_000;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();

        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = "unused",
            ["JwtTokenOption:SecurityKey"] = "integration-tests-signing-key-0123456789abcdef",
            ["ElasticSearch:Uri"] = "",
            ["Database:InitializationMode"] = "EnsureCreated",
            ["Seed:AdminEmail"] = AdminEmail,
            ["Seed:AdminPassword"] = AdminPassword,
            ["RateLimiting:Auth:PermitLimit"] = AuthPermitLimit.ToString(),
            ["HttpsRedirection:Enabled"] = "false",
        }));

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
            _connection.Dispose();
    }
}

public class RateLimitedAuthApiFactory : AuthApiFactory
{
    protected override int AuthPermitLimit => 2;
}

public class DevelopmentAuthApiFactory : AuthApiFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.UseEnvironment("Development");
    }
}
