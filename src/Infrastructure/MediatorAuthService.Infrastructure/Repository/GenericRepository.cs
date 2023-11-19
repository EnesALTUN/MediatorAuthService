using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatorAuthService.Domain.Core.Base.Abstract;
using MediatorAuthService.Domain.Core.Base.Concrete;
using MediatorAuthService.Domain.Core.Pagination;
using MediatorAuthService.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace MediatorAuthService.Infrastructure.Repository;

public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : BaseEntity, IEntity
{
    private readonly DbContext _context;
    private readonly DbSet<TEntity> _dbSet;
    private readonly IMapper _mapper;

    public GenericRepository(DbContext context, IMapper mapper)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<TEntity>();
        _mapper = mapper;
    }

    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        TEntity? entity = await _dbSet.FindAsync(new object[] { id }, cancellationToken);

        if (entity is not null)
            _context.Entry(entity).State = EntityState.Detached;

        return entity;
    }

    public async Task<TDto?> GetByIdWithProjectToAsync<TDto>(Guid id, CancellationToken cancellationToken) where TDto : BaseDto
    {
        TDto? record = await _dbSet
            .ProjectTo<TDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        return record;
    }

    public (IQueryable<TEntity>, int) GetAll(PaginationParams paginationParams, bool isNotTracking = true)
    {
        IQueryable<TEntity> query = _dbSet;

        if (isNotTracking)
            query = query.AsNoTracking();

        int count = query.Count();

        query = string.IsNullOrEmpty(paginationParams.OrderKey)
            ? query.OrderByDescending(x => x.CreatedDate)
            : query.OrderBy(paginationParams.OrderKey, paginationParams.OrderType ?? "ascending");

        query = query
            .Skip((paginationParams.PageId - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize);

        return (query, count);
    }

    public IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate) => _dbSet.Where(predicate);

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken) => await _dbSet.AddAsync(entity, cancellationToken);

    public void Remove(TEntity entity) => _dbSet.Remove(entity);

    public void Update(TEntity entity) => _context.Entry(entity).State = EntityState.Modified;

    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken) => await _dbSet.AnyAsync(predicate, cancellationToken);
}