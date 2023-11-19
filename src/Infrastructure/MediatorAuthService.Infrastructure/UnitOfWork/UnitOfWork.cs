using AutoMapper;
using MediatorAuthService.Domain.Core.Base.Abstract;
using MediatorAuthService.Domain.Core.Base.Concrete;
using MediatorAuthService.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace MediatorAuthService.Infrastructure.UnitOfWork;

public class UnitOfWork<TContext>(TContext _context, IMapper _mapper) : IUnitOfWork<TContext> where TContext : DbContext
{
    private Dictionary<Type, object>? _repositories = null;

    public TContext Context { get; } = _context ?? throw new ArgumentNullException(nameof(_context));

    public IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : BaseEntity, IEntity
    {
        return (IGenericRepository<TEntity>)GetOrAddRepository(typeof(TEntity), new GenericRepository<TEntity>(Context, _mapper));
    }

    internal object GetOrAddRepository(Type type, object repo)
    {
        _repositories ??= [];

        if (_repositories.TryGetValue(type, out var repository))
            return repository;

        _repositories.Add(type, repo);
        return repo;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken) => await Context.SaveChangesAsync(cancellationToken);

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken) => await Context.Database.BeginTransactionAsync(cancellationToken);

    public async Task CommitAsync(CancellationToken cancellationToken, bool isSaveChanges = true)
    {
        if (isSaveChanges && Context.ChangeTracker.HasChanges())
            await Context.SaveChangesAsync(cancellationToken);

        await Context.Database.CommitTransactionAsync(cancellationToken);
    }

    public async Task RollBackAsync(CancellationToken cancellationToken) => await Context.Database.RollbackTransactionAsync(cancellationToken);

    public async ValueTask DisposeAsync()
    {
        await DisposeAsync(true);

        GC.SuppressFinalize(this);
    }

    protected virtual async Task DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            _repositories?.Clear();

            await Context.DisposeAsync();
        }
    }
}