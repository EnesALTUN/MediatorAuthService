using MediatorAuthService.Application.Common.Abstractions;
using MediatorAuthService.Application.Common.Authorization;
using MediatorAuthService.Domain.Core.Extensions;
using MediatorAuthService.Domain.Entities;
using MediatorAuthService.Infrastructure.Data.Context;
using MediatorAuthService.Infrastructure.Data.Seed;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace MediatorAuthService.UnitTests;

public sealed class DatabaseInitializerTests : IDisposable
{
    private const string LegacyEmail = "admin@gmail.com";
    private const string LegacyPassword = "qwe123ASD!";

    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public DatabaseInitializerTests()
    {
        _connection.Open();

        using AppDbContext context = CreateContext();
        context.Database.EnsureCreated();
    }

    [Fact]
    public async Task DisableLegacyDefaultAdmin_DisablesTheAccount_WhenItStillUsesTheKnownPassword()
    {
        Guid stamp = await AddLegacyAdminAsync(LegacyPassword, withSession: true);

        await using (AppDbContext context = CreateContext())
            await context.DisableLegacyDefaultAdminAsync(NullLogger.Instance);

        await using AppDbContext assertContext = CreateContext();
        User admin = await assertContext.Users.Include(user => user.RefreshTokens).SingleAsync(user => user.Email == LegacyEmail);
        Assert.False(admin.IsActive);
        Assert.NotEqual(stamp, admin.SecurityStamp);
        Assert.All(admin.RefreshTokens, token => Assert.NotNull(token.RevokedAt));
    }

    [Fact]
    public async Task DisableLegacyDefaultAdmin_LeavesTheAccount_WhenThePasswordWasChanged()
    {
        Guid stamp = await AddLegacyAdminAsync("Chang3dPassw0rd!", withSession: false);

        await using (AppDbContext context = CreateContext())
            await context.DisableLegacyDefaultAdminAsync(NullLogger.Instance);

        await using AppDbContext assertContext = CreateContext();
        User admin = await assertContext.Users.SingleAsync(user => user.Email == LegacyEmail);
        Assert.True(admin.IsActive);
        Assert.Equal(stamp, admin.SecurityStamp);
    }

    [Fact]
    public async Task SeedSuperAdmin_DoesNotPromoteTheLegacyAdmin_WithoutANewPassword()
    {
        await AddLegacyAdminAsync(LegacyPassword, withSession: false);

        await using (AppDbContext context = CreateContext())
            await context.SeedSuperAdminAsync(new SeedOptions { AdminEmail = LegacyEmail }, NullLogger.Instance);

        await using AppDbContext assertContext = CreateContext();
        Assert.False(await assertContext.UserRoles.AnyAsync(userRole => userRole.RoleId == SystemRoles.SuperAdminId));
    }

    [Fact]
    public async Task SeedSuperAdmin_PromotesTheLegacyAdmin_AndReplacesItsPassword()
    {
        await AddLegacyAdminAsync(LegacyPassword, withSession: false);

        await using (AppDbContext context = CreateContext())
        {
            await context.DisableLegacyDefaultAdminAsync(NullLogger.Instance);
            await context.SeedSuperAdminAsync(new SeedOptions { AdminEmail = LegacyEmail, AdminPassword = "N3wAdm1nPassw0rd!" }, NullLogger.Instance);
        }

        await using AppDbContext assertContext = CreateContext();
        User admin = await assertContext.Users.Include(user => user.UserRoles).SingleAsync(user => user.Email == LegacyEmail);
        Assert.True(admin.IsActive);
        Assert.Contains(admin.UserRoles, userRole => userRole.RoleId == SystemRoles.SuperAdminId);
        Assert.False(HashingManager.VerifyHashedValue(admin.Password, LegacyPassword));
        Assert.True(HashingManager.VerifyHashedValue(admin.Password, "N3wAdm1nPassw0rd!"));
    }

    private async Task<Guid> AddLegacyAdminAsync(string password, bool withSession)
    {
        await using AppDbContext context = CreateContext();

        User admin = new()
        {
            Id = Guid.NewGuid(),
            Name = "Admin",
            Surname = "Admin",
            Email = LegacyEmail,
            Password = HashingManager.HashValue(password),
            IsActive = true
        };

        if (withSession)
        {
            admin.RefreshTokens.Add(new RefreshToken
            {
                Id = Guid.NewGuid(),
                TokenHash = Guid.NewGuid().ToString("N"),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            });
        }

        context.Users.Add(admin);
        await context.SaveChangesAsync();

        return admin.SecurityStamp;
    }

    private AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options, new NoCurrentUser());

    public void Dispose() => _connection.Dispose();

    private sealed class NoCurrentUser : ICurrentUserService
    {
        public Guid? UserId => null;
    }
}
