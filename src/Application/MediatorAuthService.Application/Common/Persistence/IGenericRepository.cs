using MediatorAuthService.Domain.Core.Pagination;
using System.Linq.Expressions;

namespace MediatorAuthService.Application.Common.Persistence;

public interface IGenericRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// Returns the tracked entity with the given ID, or null. Soft-deleted records are excluded.
    /// </summary>
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Projects the record with the given ID without tracking it.
    /// </summary>
    Task<TResult?> GetByIdAsync<TResult>(Guid id, Expression<Func<TEntity, TResult>> selector, CancellationToken cancellationToken);

    Task<(List<TResult> Items, int TotalCount)> GetPagedAsync<TResult>(
        PaginationParams paginationParams,
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Read-only (no-tracking) query over all records.
    /// </summary>
    IQueryable<TEntity> Query();

    IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate);

    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);

    Task AddAsync(TEntity entity, CancellationToken cancellationToken);

    void Remove(TEntity entity);

    void RemoveRange(IEnumerable<TEntity> entities);
}
