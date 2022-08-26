using MediatorAuthService.Domain.Core.Base.Concrete;
using MediatorAuthService.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

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
        // It will be unlocked when the JWT Token is activated.
        //Guid userId = Guid.Parse(_httpContextAccessor.HttpContext.User.FindFirst(x => x.Type == ClaimTypes.NameIdentifier)?.Value ?? "");

        Guid currentUserId = Guid.NewGuid();

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
                case EntityState.Deleted:
                    baseEntity.DeletedDate = DateTime.Now;
                    baseEntity.DeletedUserId = currentUserId;
                    break;
            }
        });

        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public DbSet<User> Users { get; set; }
}