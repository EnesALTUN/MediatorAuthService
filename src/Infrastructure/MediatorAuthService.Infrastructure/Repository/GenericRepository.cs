using MediatorAuthService.Application.Common.Persistence;
using MediatorAuthService.Domain.Core.Base.Concrete;
using MediatorAuthService.Domain.Core.Pagination;
using MediatorAuthService.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace MediatorAuthService.Infrastructure.Repository;

public class GenericRepository<TEntity>(DbContext context) : IGenericRepository<TEntity> where TEntity : class
{
    private const string IdPropertyName = "Id";

    private readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();

    // Queried instead of DbSet.Find so that global query filters (soft delete) always apply.
    public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _dbSet.SingleOrDefaultAsync(entity => EF.Property<Guid>(entity, IdPropertyName) == id, cancellationToken);

    public Task<TResult?> GetByIdAsync<TResult>(Guid id, Expression<Func<TEntity, TResult>> selector, CancellationToken cancellationToken) =>
        _dbSet
            .AsNoTracking()
            .Where(entity => EF.Property<Guid>(entity, IdPropertyName) == id)
            .Select(selector)
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<(List<TResult> Items, int TotalCount)> GetPagedAsync<TResult>(
        PaginationParams paginationParams,
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = _dbSet.AsNoTracking();

        if (predicate is not null)
            query = query.Where(predicate);

        int totalCount = await query.CountAsync(cancellationToken);

        IQueryable<TEntity> orderedQuery = string.IsNullOrEmpty(paginationParams.OrderKey)
            ? query.OrderBy(nameof(BaseEntity.CreatedDate), "descending")
            : query.OrderBy(paginationParams.OrderKey, paginationParams.OrderType ?? "ascending");

        List<TResult> items = await orderedQuery
            .Skip((paginationParams.PageId - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .Select(selector)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public IQueryable<TEntity> Query() => _dbSet.AsNoTracking();

    public IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate) => _dbSet.Where(predicate);

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken) => await _dbSet.AddAsync(entity, cancellationToken);

    public void Remove(TEntity entity) => _dbSet.Remove(entity);

    public void RemoveRange(IEnumerable<TEntity> entities) => _dbSet.RemoveRange(entities);

    public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken) => _dbSet.AnyAsync(predicate, cancellationToken);
}
