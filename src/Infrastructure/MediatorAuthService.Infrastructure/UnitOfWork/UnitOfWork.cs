using AutoMapper;
using MediatorAuthService.Domain.Core.Base.Abstract;
using MediatorAuthService.Domain.Core.Base.Concrete;
using MediatorAuthService.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace MediatorAuthService.Infrastructure.UnitOfWork;

public class UnitOfWork<TContext> : IUnitOfWork<TContext>, IUnitOfWork where TContext : DbContext
{
    private Dictionary<Type, object> _repositories;
    private readonly IMapper _mapper;

    public TContext Context { get; }

    public UnitOfWork(TContext context, IMapper mapper)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        _mapper = mapper;
    }

    public IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : BaseEntity, IEntity
    {
        return (IGenericRepository<TEntity>)GetOrAddRepository(typeof(TEntity), new GenericRepository<TEntity>(Context, _mapper));
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
            if (_repositories != null)
                _repositories.Clear();

            await Context.DisposeAsync();
        }
    }
}