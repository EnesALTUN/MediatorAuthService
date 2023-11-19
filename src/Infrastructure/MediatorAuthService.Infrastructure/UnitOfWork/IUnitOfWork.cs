using MediatorAuthService.Domain.Core.Base.Abstract;
using MediatorAuthService.Domain.Core.Base.Concrete;
using MediatorAuthService.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace MediatorAuthService.Infrastructure.UnitOfWork;

public interface IUnitOfWork : IAsyncDisposable
{
    IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : BaseEntity, IEntity;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken);

    Task CommitAsync(CancellationToken cancellationToken, bool isSaveChanges = true);

    Task RollBackAsync(CancellationToken cancellationToken);
}

public interface IUnitOfWork<out TContext> : IUnitOfWork where TContext : DbContext
{
    TContext Context { get; }
}