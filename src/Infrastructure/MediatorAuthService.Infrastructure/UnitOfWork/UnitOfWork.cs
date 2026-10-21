using MediatorAuthService.Application.Common.Persistence;
using MediatorAuthService.Application.Exceptions;
using MediatorAuthService.Infrastructure.Repository;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace MediatorAuthService.Infrastructure.UnitOfWork;

/// <summary>
/// Unit of work bound to a single DbContext. The context is owned and disposed by the DI container.
/// </summary>
public class UnitOfWork<TContext>(TContext _context) : IUnitOfWork where TContext : DbContext
{
    private readonly Dictionary<Type, object> _repositories = [];

    public IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : class
    {
        if (!_repositories.TryGetValue(typeof(TEntity), out object? repository))
        {
            repository = new GenericRepository<TEntity>(_context);
            _repositories.Add(typeof(TEntity), repository);
        }

        return (IGenericRepository<TEntity>)repository;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            throw new ConflictException("The record conflicts with an existing record.", ex);
        }
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken) => await _context.Database.BeginTransactionAsync(cancellationToken);

    public async Task CommitAsync(CancellationToken cancellationToken, bool isSaveChanges = true)
    {
        if (isSaveChanges && _context.ChangeTracker.HasChanges())
            await SaveChangesAsync(cancellationToken);

        await _context.Database.CommitTransactionAsync(cancellationToken);
    }

    public async Task RollBackAsync(CancellationToken cancellationToken) => await _context.Database.RollbackTransactionAsync(cancellationToken);

    // 2601: duplicate key in unique index, 2627: unique constraint violation.
    private static bool IsUniqueConstraintViolation(DbUpdateException ex) => ex.InnerException is SqlException { Number: 2601 or 2627 };
}
