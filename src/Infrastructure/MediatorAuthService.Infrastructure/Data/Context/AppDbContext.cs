using MediatorAuthService.Application.Common.Abstractions;
using MediatorAuthService.Domain.Core.Base.Concrete;
using MediatorAuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq.Expressions;

namespace MediatorAuthService.Infrastructure.Data.Context;

public class AppDbContext(DbContextOptions<AppDbContext> _options, ICurrentUserService _currentUserService) : DbContext(_options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);

        ApplySoftDeleteQueryFilters(modelBuilder);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAuditInformation();

        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ApplyAuditInformation();

        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>
    /// Fills the audit columns of BaseEntity records and turns deletes into soft deletes.
    /// Other entities (join tables, refresh tokens) are deleted physically.
    /// </summary>
    private void ApplyAuditInformation()
    {
        Guid currentUserId = _currentUserService.UserId ?? Guid.Empty;
        DateTime now = DateTime.UtcNow;

        foreach (EntityEntry<BaseEntity> entry in ChangeTracker.Entries<BaseEntity>())
        {
            BaseEntity baseEntity = entry.Entity;

            switch (entry.State)
            {
                case EntityState.Added:
                    baseEntity.CreatedDate = now;
                    baseEntity.CreatedUserId = currentUserId;
                    baseEntity.IsActive = true;
                    break;
                case EntityState.Modified:
                    baseEntity.ModifiedDate = now;
                    baseEntity.ModifiedUserId = currentUserId;
                    entry.Property(x => x.CreatedDate).IsModified = false;
                    entry.Property(x => x.CreatedUserId).IsModified = false;
                    break;
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    baseEntity.DeletedDate = now;
                    baseEntity.DeletedUserId = currentUserId;
                    baseEntity.IsActive = false;
                    break;
            }
        }
    }

    /// <summary>
    /// Hides soft-deleted records (DeletedDate is set) from every query of BaseEntity types.
    /// </summary>
    private static void ApplySoftDeleteQueryFilters(ModelBuilder modelBuilder)
    {
        foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes().Where(type => typeof(BaseEntity).IsAssignableFrom(type.ClrType)))
        {
            ParameterExpression parameter = Expression.Parameter(entityType.ClrType, "entity");

            LambdaExpression filter = Expression.Lambda(
                Expression.Equal(
                    Expression.Property(parameter, nameof(BaseEntity.DeletedDate)),
                    Expression.Constant(null, typeof(DateTime?))),
                parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
        }
    }
}
