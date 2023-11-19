using MediatorAuthService.Application.Extensions;
using MediatorAuthService.Domain.Core.Base.Concrete;
using MediatorAuthService.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace MediatorAuthService.Infrastructure.Data.Context;

public class AppDbContext(DbContextOptions<AppDbContext> _options, IHttpContextAccessor _httpContextAccessor) : DbContext(_options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        Guid currentUserId = _httpContextAccessor.HttpContext.User.Id();

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
                    e.State = EntityState.Modified;
                    baseEntity.DeletedDate = DateTime.Now;
                    baseEntity.DeletedUserId = currentUserId;
                    baseEntity.IsActive = false;
                    break;
            }
        });

        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public DbSet<User> Users { get; set; }
}