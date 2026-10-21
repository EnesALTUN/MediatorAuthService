using MediatorAuthService.Application.Dtos.AuthDtos;
using MediatorAuthService.Application.Dtos.UserDtos;
using MediatorAuthService.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;

namespace MediatorAuthService.IntegrationTests;

public abstract class IntegrationTestBase(AuthApiFactory factory) : IClassFixture<AuthApiFactory>
{
    protected AuthApiFactory Factory { get; } = factory;

    protected HttpClient CreateClient() => Factory.CreateClient();

    protected async Task<HttpClient> CreateAdminClientAsync()
    {
        HttpClient client = CreateClient();
        TokenDto token = await client.LoginSuccessfullyAsync(AuthApiFactory.AdminEmail, AuthApiFactory.AdminPassword);
        return client.WithToken(token.AccessToken);
    }

    /// <summary>
    /// Creates a user with a role holding exactly the given permissions and returns a client signed in as that user.
    /// </summary>
    protected async Task<(HttpClient Client, UserDto User)> CreateUserClientAsync(params string[] permissions)
    {
        HttpClient adminClient = await CreateAdminClientAsync();
        UserDto user = await adminClient.CreateUserAsync(ApiClientExtensions.UniqueEmail());

        if (permissions.Length != 0)
        {
            Guid roleId = (await adminClient.CreateRoleAsync(permissions)).Id;
            Assert.Equal(HttpStatusCode.OK, (await adminClient.SetRolesAsync(user.Id, roleId)).StatusCode);
        }

        HttpClient client = CreateClient();
        TokenDto token = await client.LoginSuccessfullyAsync(user.Email, ApiClientExtensions.DefaultPassword);

        return (client.WithToken(token.AccessToken), user);
    }

    protected static Task<HttpResponseMessage> GetMeAsync(HttpClient client) => client.GetAsync("/api/v1/Auth/me");

    protected static Task<HttpResponseMessage> UpdateUserAsync(HttpClient client, UserDto user, bool? isActive = null) =>
        client.PutAsJsonAsync($"/api/v1/User/{user.Id}", new { name = user.Name, surname = user.Surname, email = user.Email, isActive });
}
