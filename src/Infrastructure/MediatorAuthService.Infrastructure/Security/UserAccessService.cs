using MediatorAuthService.Application.Common.Abstractions;
using MediatorAuthService.Application.Common.Authorization;
using MediatorAuthService.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using System.Collections.Concurrent;

namespace MediatorAuthService.Infrastructure.Security;

/// <summary>
/// Loads user access snapshots from the database and caches them in memory.
/// Handlers that change users, roles or permissions invalidate the cache, so changes apply on the next request.
/// In a multi-instance deployment other instances pick changes up when their entries expire.
/// </summary>
internal sealed class UserAccessService(IServiceScopeFactory _scopeFactory, IMemoryCache _cache) : IUserAccessService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private CancellationTokenSource _resetTokenSource = new();

    // One source per user, so that Invalidate also expires an entry whose load was already in flight.
    // Entries stay small and are bounded by the number of users who have made a request.
    private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _userTokenSources = new();

    public async Task<UserAccessSnapshot?> GetAsync(Guid userId, CancellationToken cancellationToken)
    {
        string cacheKey = CacheKey(userId);

        if (_cache.TryGetValue(cacheKey, out UserAccessSnapshot? snapshot))
            return snapshot;

        // Captured before loading so that an invalidation during the load expires the entry immediately.
        CancellationToken resetToken = _resetTokenSource.Token;
        CancellationToken userToken = _userTokenSources.GetOrAdd(userId, _ => new CancellationTokenSource()).Token;

        snapshot = await LoadAsync(userId, cancellationToken);

        _cache.Set(cacheKey, snapshot, new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(CacheDuration)
            .AddExpirationToken(new CancellationChangeToken(resetToken))
            .AddExpirationToken(new CancellationChangeToken(userToken)));

        return snapshot;
    }

    public void Invalidate(Guid userId)
    {
        if (_userTokenSources.TryRemove(userId, out CancellationTokenSource? previous))
            previous.Cancel();

        _cache.Remove(CacheKey(userId));
    }

    public void InvalidateAll()
    {
        CancellationTokenSource previous = Interlocked.Exchange(ref _resetTokenSource, new CancellationTokenSource());
        previous.Cancel();
    }

    private async Task<UserAccessSnapshot?> LoadAsync(Guid userId, CancellationToken cancellationToken)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = await context.Users
            .AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user => new { user.IsActive, user.SecurityStamp })
            .SingleOrDefaultAsync(cancellationToken);

        if (user is null)
            return null;

        var roles = await context.UserRoles
            .AsNoTracking()
            .Where(userRole => userRole.UserId == userId)
            .Select(userRole => new
            {
                userRole.RoleId,
                Permissions = userRole.Role.Permissions.Select(permission => permission.Permission).ToList()
            })
            .ToListAsync(cancellationToken);

        bool isSuperAdmin = roles.Any(role => role.RoleId == SystemRoles.SuperAdminId);

        HashSet<string> permissions = isSuperAdmin
            ? [.. Permissions.AllNames]
            : [.. roles.SelectMany(role => role.Permissions)];

        return new UserAccessSnapshot(userId, user.IsActive, user.SecurityStamp, isSuperAdmin, [.. roles.Select(role => role.RoleId)], permissions);
    }

    private static string CacheKey(Guid userId) => $"user-access:{userId}";
}
