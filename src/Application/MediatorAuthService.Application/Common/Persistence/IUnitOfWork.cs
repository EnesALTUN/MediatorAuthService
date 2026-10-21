using Microsoft.EntityFrameworkCore.Storage;

namespace MediatorAuthService.Application.Common.Persistence;

public interface IUnitOfWork
{
    IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : class;

    /// <summary>
    /// Persists changes. Unique constraint violations are translated into a ConflictException.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken);

    Task CommitAsync(CancellationToken cancellationToken, bool isSaveChanges = true);

    Task RollBackAsync(CancellationToken cancellationToken);
}
