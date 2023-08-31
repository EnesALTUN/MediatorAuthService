using MediatorAuthService.Domain.Core.Base.Abstract;
using MediatorAuthService.Domain.Core.Base.Concrete;
using MediatorAuthService.Domain.Core.Pagination;
using System.Linq.Expressions;

namespace MediatorAuthService.Infrastructure.Repository;

public interface IGenericRepository<TEntity> where TEntity : IEntity
{
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<TDto?> GetByIdWithProjectToAsync<TDto>(Guid id, CancellationToken cancellationToken) where TDto : BaseDto;

    (IQueryable<TEntity>, int) GetAll(PaginationParams paginationParams, bool isNotTracking = true);

    IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate);

    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);

    Task AddAsync(TEntity entity, CancellationToken cancellationToken);

    void Remove(TEntity entity);

    void Update(TEntity entity);
}