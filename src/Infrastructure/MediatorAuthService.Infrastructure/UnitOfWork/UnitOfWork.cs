using MediatorAuthService.Domain.Core.Base.Abstract;
using MediatorAuthService.Domain.Core.Base.Concrete;
using MediatorAuthService.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace MediatorAuthService.Infrastructure.UnitOfWork;

public class UnitOfWork<TContext> : IUnitOfWork<TContext>, IUnitOfWork where TContext : DbContext
{
    private Dictionary<Type, object> _repositories;

    public TContext Context { get; }

    public UnitOfWork(TContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : BaseEntity, IEntity
    {
        return (IGenericRepository<TEntity>)GetOrAddRepository(typeof(TEntity), new GenericRepository<TEntity>(Context));
    }

    internal object GetOrAddRepository(Type type, object repo)
    {
        if (_repositories is null)
            _repositories = new Dictionary<Type, object>();

        if (_repositories.TryGetValue(type, out object repository))
            return repository;

        _repositories.Add(type, repo);
        return repo;
    }

    public async Task<int> SaveChangesAsync() => await Context.SaveChangesAsync();

    public async Task<IDbContextTransaction> BeginTransactionAsync() => await Context.Database.BeginTransactionAsync();

    public async Task CommitAsync(bool isSaveChanges = true)
    {
        if (isSaveChanges && Context.ChangeTracker.HasChanges())
            await Context.SaveChangesAsync();

        await Context.Database.CommitTransactionAsync();
    }

    public async Task RollBackAsync() => await Context.Database.RollbackTransactionAsync();

    public async ValueTask DisposeAsync()
    {
        await DisposeAsync(true);

        GC.SuppressFinalize(this);
    }

    protected virtual async Task DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            if (_repositories != null)
                _repositories.Clear();

            await Context.DisposeAsync();
        }
    }
}