using MediatorAuthService.Application.Common.Abstractions;
using MediatorAuthService.Domain.Entities;
using MediatorAuthService.Infrastructure.Data.Context;
using MediatorAuthService.Infrastructure.Security;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;

namespace MediatorAuthService.UnitTests;

public sealed class UserAccessServiceTests : IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly BlockingInterceptor _interceptor = new();
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());
    private readonly ServiceProvider _provider;

    public UserAccessServiceTests()
    {
        _connection.Open();

        _provider = new ServiceCollection()
            .AddSingleton<ICurrentUserService, NoCurrentUser>()
            .AddDbContext<AppDbContext>(options => options.UseSqlite(_connection).AddInterceptors(_interceptor))
            .BuildServiceProvider();

        using IServiceScope scope = _provider.CreateScope();
        scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
    }

    [Fact]
    public async Task GetAsync_CachesTheSnapshot()
    {
        Guid userId = await AddUserAsync();
        UserAccessService service = new(_provider.GetRequiredService<IServiceScopeFactory>(), _cache);

        await service.GetAsync(userId, CancellationToken.None);

        Assert.True(_cache.TryGetValue(CacheKey(userId), out _));
    }

    [Fact]
    public async Task Invalidate_DuringAnInFlightLoad_PreventsTheStaleSnapshotFromBeingCached()
    {
        Guid userId = await AddUserAsync();
        UserAccessService service = new(_provider.GetRequiredService<IServiceScopeFactory>(), _cache);

        _interceptor.Block();
        Task load = service.GetAsync(userId, CancellationToken.None);
        await _interceptor.Started.Task.WaitAsync(TimeSpan.FromSeconds(10));

        service.Invalidate(userId);
        _interceptor.Release();
        await load;

        Assert.False(_cache.TryGetValue(CacheKey(userId), out _));
    }

    private async Task<Guid> AddUserAsync()
    {
        using IServiceScope scope = _provider.CreateScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        User user = new()
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Surname = "User",
            Email = $"{Guid.NewGuid():N}@test.local",
            Password = "hash"
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return user.Id;
    }

    private static string CacheKey(Guid userId) => $"user-access:{userId}";

    public void Dispose()
    {
        _provider.Dispose();
        _cache.Dispose();
        _connection.Dispose();
    }

    private sealed class NoCurrentUser : ICurrentUserService
    {
        public Guid? UserId => null;
    }

    /// <summary>
    /// Holds the next query until it is released, so that a test can act while a load is in flight.
    /// </summary>
    private sealed class BlockingInterceptor : DbCommandInterceptor
    {
        private readonly TaskCompletionSource _release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int _armed;

        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public void Block() => Interlocked.Exchange(ref _armed, 1);

        public void Release() => _release.TrySetResult();

        public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            // Only the first query after Block() waits.
            if (Interlocked.Exchange(ref _armed, 0) == 1)
            {
                Started.TrySetResult();
                await _release.Task;
            }

            return result;
        }
    }
}
