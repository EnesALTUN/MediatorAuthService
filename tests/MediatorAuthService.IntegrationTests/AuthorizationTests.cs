using MediatorAuthService.Application.Common.Authorization;
using MediatorAuthService.Application.Dtos.AuthDtos;
using MediatorAuthService.Application.Dtos.RoleDtos;
using MediatorAuthService.Application.Dtos.UserDtos;
using MediatorAuthService.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;

namespace MediatorAuthService.IntegrationTests;

public class AuthorizationTests(AuthApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task AnonymousRequests_AreRejectedWithTheApiEnvelope()
    {
        HttpResponseMessage response = await CreateClient().GetAsync("/api/v1/User");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Single(await response.ReadErrorsAsync());
    }

    [Fact]
    public async Task PermissionChanges_ApplyImmediately_WithoutANewToken()
    {
        (HttpClient client, UserDto user) = await CreateUserClientAsync();

        HttpResponseMessage forbidden = await client.GetAsync("/api/v1/User");
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
        Assert.Single(await forbidden.ReadErrorsAsync());

        HttpClient adminClient = await CreateAdminClientAsync();
        RoleDto role = await adminClient.CreateRoleAsync(Permissions.Users.Read);
        Assert.Equal(HttpStatusCode.OK, (await adminClient.SetRolesAsync(user.Id, role.Id)).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/v1/User")).StatusCode);

        // Removing the permission from the role revokes it for the user as well.
        HttpResponseMessage update = await adminClient.PutAsJsonAsync($"/api/v1/Role/{role.Id}", new { name = role.Name, permissions = Array.Empty<string>() });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);

        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/v1/User")).StatusCode);
    }

    [Fact]
    public async Task Users_CanReadAndUpdateThemselves_ButNotOthers()
    {
        (HttpClient client, UserDto user) = await CreateUserClientAsync();
        UserDto other = await (await CreateAdminClientAsync()).CreateUserAsync(ApiClientExtensions.UniqueEmail());

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/v1/User/{user.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync($"/api/v1/User/{other.Id}")).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await UpdateUserAsync(client, user)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await UpdateUserAsync(client, other)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await UpdateUserAsync(client, user, isActive: false)).StatusCode);
    }

    [Fact]
    public async Task DeactivatedUser_CannotUseExistingTokens_OrSignIn()
    {
        (HttpClient client, UserDto user) = await CreateUserClientAsync();
        HttpClient adminClient = await CreateAdminClientAsync();

        Assert.Equal(HttpStatusCode.OK, (await UpdateUserAsync(adminClient, user, isActive: false)).StatusCode);

        Assert.Equal(HttpStatusCode.Unauthorized, (await GetMeAsync(client)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await CreateClient().LoginAsync(user.Email, ApiClientExtensions.DefaultPassword)).StatusCode);
    }

    [Fact]
    public async Task Roles_CannotGrantPermissionsTheCallerDoesNotHold()
    {
        (HttpClient client, _) = await CreateUserClientAsync(Permissions.Roles.Create, Permissions.Roles.Read);

        HttpResponseMessage escalation = await client.PostAsJsonAsync("/api/v1/Role", new { name = $"r-{Guid.NewGuid():N}", permissions = new[] { Permissions.Users.Delete } });
        Assert.Equal(HttpStatusCode.Forbidden, escalation.StatusCode);

        HttpResponseMessage allowed = await client.PostAsJsonAsync("/api/v1/Role", new { name = $"r-{Guid.NewGuid():N}", permissions = new[] { Permissions.Roles.Read } });
        Assert.Equal(HttpStatusCode.Created, allowed.StatusCode);
    }

    [Fact]
    public async Task OnlySuperAdmins_CanAssignTheSuperAdminRole()
    {
        (HttpClient client, UserDto user) = await CreateUserClientAsync(Permissions.Users.AssignRoles);

        HttpResponseMessage response = await client.SetRolesAsync(user.Id, SystemRoles.SuperAdminId);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task TheLastSuperAdmin_CannotLoseTheRole()
    {
        HttpClient adminClient = await CreateAdminClientAsync();
        CurrentUserDto admin = await (await GetMeAsync(adminClient)).ReadDataAsync<CurrentUserDto>();

        HttpResponseMessage response = await adminClient.SetRolesAsync(admin.Id);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("At least one active SuperAdmin", (await response.ReadErrorsAsync()).Single());
    }

    [Fact]
    public async Task TheLastActiveSuperAdmin_CannotLoseTheRole_WhenOtherSuperAdminsAreInactive()
    {
        HttpClient adminClient = await CreateAdminClientAsync();
        CurrentUserDto admin = await (await GetMeAsync(adminClient)).ReadDataAsync<CurrentUserDto>();
        UserDto other = await adminClient.CreateUserAsync(ApiClientExtensions.UniqueEmail());
        Assert.Equal(HttpStatusCode.OK, (await adminClient.SetRolesAsync(other.Id, SystemRoles.SuperAdminId)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await UpdateUserAsync(adminClient, other, isActive: false)).StatusCode);

        HttpResponseMessage response = await adminClient.SetRolesAsync(admin.Id);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("At least one active SuperAdmin", (await response.ReadErrorsAsync()).Single());
    }

    [Fact]
    public async Task SystemRoles_CannotBeModifiedOrDeleted()
    {
        HttpClient adminClient = await CreateAdminClientAsync();

        HttpResponseMessage update = await adminClient.PutAsJsonAsync($"/api/v1/Role/{SystemRoles.SuperAdminId}", new { name = "Renamed", permissions = Array.Empty<string>() });
        HttpResponseMessage delete = await adminClient.DeleteAsync($"/api/v1/Role/{SystemRoles.SuperAdminId}");

        Assert.Equal(HttpStatusCode.Forbidden, update.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, delete.StatusCode);
    }

    [Fact]
    public async Task UsersWithFewerPermissions_CannotManageMorePrivilegedUsers()
    {
        (HttpClient client, _) = await CreateUserClientAsync(Permissions.Users.Delete, Permissions.Users.Update);
        CurrentUserDto admin = await (await GetMeAsync(await CreateAdminClientAsync())).ReadDataAsync<CurrentUserDto>();

        Assert.Equal(HttpStatusCode.Forbidden, (await client.DeleteAsync($"/api/v1/User/{admin.Id}")).StatusCode);
    }

    [Fact]
    public async Task DeletingARole_RemovesItsPermissionsFromUsers()
    {
        HttpClient adminClient = await CreateAdminClientAsync();
        (HttpClient client, UserDto user) = await CreateUserClientAsync();
        RoleDto role = await adminClient.CreateRoleAsync(Permissions.Roles.Read);
        await adminClient.SetRolesAsync(user.Id, role.Id);

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/v1/Permission")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await adminClient.DeleteAsync($"/api/v1/Role/{role.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/v1/Permission")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await adminClient.GetAsync($"/api/v1/Role/{role.Id}")).StatusCode);
    }

    [Fact]
    public async Task Roles_AreListedWithEffectivePermissions()
    {
        List<RoleDto> roles = await (await (await CreateAdminClientAsync()).GetAsync("/api/v1/Role")).ReadDataAsync<List<RoleDto>>();

        RoleDto superAdmin = roles.First();
        Assert.Equal(SystemRoles.SuperAdminId, superAdmin.Id);
        Assert.True(superAdmin.IsSystem);
        Assert.Equal(Permissions.AllNames.Count, superAdmin.Permissions.Count);
    }
}
