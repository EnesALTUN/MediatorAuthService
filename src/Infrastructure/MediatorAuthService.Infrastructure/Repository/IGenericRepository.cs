using MediatorAuthService.Domain.Core.Base.Abstract;
using MediatorAuthService.Domain.Core.Pagination;
using System.Linq.Expressions;

namespace MediatorAuthService.Infrastructure.Repository;

public interface IGenericRepository<TEntity> where TEntity : IEntity
{
    Task<TEntity> GetByIdAsync(Guid id);

    (IQueryable<TEntity>, int) GetAll(PaginationParams paginationParams);

    IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate);

    Task AddAsync(TEntity entity);

    void Remove(TEntity entity);

    Task<TEntity> UpdateAsync(TEntity entity);
}