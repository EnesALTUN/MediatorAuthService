using MediatorAuthService.Domain.Core.Base.Concrete;
using MediatorAuthService.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MediatorAuthService.Infrastructure.Data.Context;

public class AppDbContext : DbContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        Guid currentUserId = Guid.Parse(_httpContextAccessor.HttpContext.User.FindFirst(x => x.Type == ClaimTypes.NameIdentifier)?.Value!);

        ChangeTracker.Entries().ToList().ForEach(e =>
        {
            BaseEntity baseEntity = (BaseEntity)e.Entity;

            switch (e.State)
            {
                case EntityState.Added:
                    baseEntity.CreatedDate = DateTime.Now;
                    baseEntity.CreatedUserId = currentUserId;
                    baseEntity.IsActive = true;
                    break;
                case EntityState.Modified:
                    baseEntity.ModifiedDate = DateTime.Now;
                    baseEntity.ModifiedUserId = currentUserId;
                    break;
            }
        });

        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public DbSet<User> Users { get; set; }
}