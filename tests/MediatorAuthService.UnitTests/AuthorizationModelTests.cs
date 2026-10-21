using MediatorAuthService.Application.Common.Authorization;
using MediatorAuthService.Application.Common.Extensions;
using MediatorAuthService.Application.Exceptions;
using MediatorAuthService.Domain.Entities;
using MediatorAuthService.Infrastructure.Extensions;
using System.Linq.Expressions;

namespace MediatorAuthService.UnitTests;

public class AuthorizationModelTests
{
    [Fact]
    public void SuperAdmin_HasEveryPermission()
    {
        UserAccessSnapshot snapshot = new(Guid.NewGuid(), true, Guid.NewGuid(), IsSuperAdmin: true, [], new HashSet<string>());

        Assert.True(snapshot.HasPermission(Permissions.Users.Delete));
        Assert.True(snapshot.HasAllPermissions(Permissions.AllNames));
    }

    [Fact]
    public void RegularUser_HasOnlyGrantedPermissions()
    {
        UserAccessSnapshot snapshot = new(Guid.NewGuid(), true, Guid.NewGuid(), IsSuperAdmin: false, [], new HashSet<string> { Permissions.Users.Read });

        Assert.True(snapshot.HasPermission(Permissions.Users.Read));
        Assert.False(snapshot.HasPermission(Permissions.Users.Delete));
        Assert.False(snapshot.HasAllPermissions([Permissions.Users.Read, Permissions.Users.Delete]));
    }

    [Fact]
    public void PermissionCatalog_HasUniqueNames()
    {
        Assert.Equal(Permissions.All.Count, Permissions.AllNames.Count);
        Assert.True(Permissions.Exists(Permissions.Roles.Update));
        Assert.False(Permissions.Exists("Users.Everything"));
    }

    [Fact]
    public void PredicateBuilder_And_CombinesBothConditions()
    {
        Expression<Func<int, bool>> predicate = PredicateBuilder.True<int>().And(x => x > 1).And(x => x < 5);

        Func<int, bool> compiled = predicate.Compile();

        Assert.Equal([2, 3, 4], Enumerable.Range(0, 7).Where(compiled));
        Assert.Single(predicate.Parameters);
    }

    [Fact]
    public void OrderBy_IsCaseInsensitive_AndRejectsNotSortableProperties()
    {
        IQueryable<User> users = new[]
        {
            new User { Name = "b", Surname = "x", Email = "b@x.com", Password = "-" },
            new User { Name = "a", Surname = "x", Email = "a@x.com", Password = "-" },
        }.AsQueryable();

        Assert.Equal(["a", "b"], users.OrderBy("name", "ascending").Select(user => user.Name));
        Assert.Equal(["b", "a"], users.OrderBy("Name", "DESCENDING").Select(user => user.Name));
        Assert.Throws<BusinessException>(() => users.OrderBy("Password", "ascending"));
        Assert.Throws<BusinessException>(() => users.OrderBy("Unknown", "ascending"));
    }
}
