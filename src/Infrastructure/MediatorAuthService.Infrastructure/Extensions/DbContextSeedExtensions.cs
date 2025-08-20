using MediatorAuthService.Domain.Core.Extensions;
using MediatorAuthService.Domain.Entities;
using MediatorAuthService.Infrastructure.Data.Context;

namespace MediatorAuthService.Infrastructure.Extensions;

public static class DbContextSeedExtensions
{
    public static async Task SeedAdminUserAsync(this AppDbContext context)
    {
        if (!context.Users.Any(u => u.Email == "admin@gmail.com"))
        {
            User adminUser = new()
            {
                Id = Guid.Parse("d0bfa391-a604-4049-a868-359091461e46"),
                Email = "admin@gmail.com",
                Password = HashingManager.HashValue("qwe123"),
                Name = "Admin",
                Surname = "Admin",
                RefreshToken = HashingManager.HashValue(Guid.NewGuid().ToString())
            };
            context.Users.Add(adminUser);

            await context.SaveChangesAsync();
        }
    }
}