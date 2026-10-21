using MediatorAuthService.Application.Common.Authorization;
using MediatorAuthService.Application.Dtos.AuthDtos;
using MediatorAuthService.Application.Dtos.UserDtos;
using MediatorAuthService.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;

namespace MediatorAuthService.IntegrationTests;

public class AuthFlowTests(AuthApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Login_ReturnsAccessAndRefreshTokens()
    {
        TokenDto token = await CreateClient().LoginSuccessfullyAsync(AuthApiFactory.AdminEmail, AuthApiFactory.AdminPassword);

        Assert.Equal("Bearer", token.TokenType);
        Assert.False(token.AccessToken.StartsWith("Bearer", StringComparison.Ordinal));
        Assert.False(string.IsNullOrEmpty(token.RefreshToken));
        Assert.True(token.AccessTokenExpire < token.RefreshTokenExpire);
    }

    [Fact]
    public async Task Login_WithUnknownEmailOrWrongPassword_ReturnsTheSameError()
    {
        HttpClient client = CreateClient();

        HttpResponseMessage unknown = await client.LoginAsync(ApiClientExtensions.UniqueEmail(), "Whatever1!");
        HttpResponseMessage wrongPassword = await client.LoginAsync(AuthApiFactory.AdminEmail, "Wrong1!");

        Assert.Equal(HttpStatusCode.Unauthorized, unknown.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);
        Assert.Equal(await unknown.ReadErrorsAsync(), await wrongPassword.ReadErrorsAsync());
    }

    [Fact]
    public async Task Login_LocksTheAccount_AfterRepeatedFailures()
    {
        HttpClient adminClient = await CreateAdminClientAsync();
        UserDto user = await adminClient.CreateUserAsync(ApiClientExtensions.UniqueEmail());
        HttpClient client = CreateClient();

        for (int attempt = 0; attempt < 5; attempt++)
            Assert.Equal(HttpStatusCode.Unauthorized, (await client.LoginAsync(user.Email, "Wrong1!")).StatusCode);

        HttpResponseMessage locked = await client.LoginAsync(user.Email, ApiClientExtensions.DefaultPassword);

        Assert.Equal(HttpStatusCode.Unauthorized, locked.StatusCode);
        Assert.Contains("Too many failed login attempts", (await locked.ReadErrorsAsync()).Single());
    }

    [Fact]
    public async Task RefreshToken_WorksWithoutAnAccessToken_AndRotatesTheToken()
    {
        TokenDto token = await CreateClient().LoginSuccessfullyAsync(AuthApiFactory.AdminEmail, AuthApiFactory.AdminPassword);

        HttpResponseMessage response = await CreateClient().RefreshAsync(token.RefreshToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        TokenDto refreshed = await response.ReadDataAsync<TokenDto>();
        Assert.NotEqual(token.RefreshToken, refreshed.RefreshToken);
        Assert.Equal(HttpStatusCode.OK, (await GetMeAsync(CreateClient().WithToken(refreshed.AccessToken))).StatusCode);
    }

    [Fact]
    public async Task RefreshToken_Reuse_RevokesEverySessionOfTheUser()
    {
        HttpClient adminClient = await CreateAdminClientAsync();
        UserDto user = await adminClient.CreateUserAsync(ApiClientExtensions.UniqueEmail());
        HttpClient client = CreateClient();

        TokenDto first = await client.LoginSuccessfullyAsync(user.Email, ApiClientExtensions.DefaultPassword);
        TokenDto second = await (await client.RefreshAsync(first.RefreshToken)).ReadDataAsync<TokenDto>();

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.RefreshAsync(first.RefreshToken)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.RefreshAsync(second.RefreshToken)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await GetMeAsync(CreateClient().WithToken(second.AccessToken))).StatusCode);
    }

    [Fact]
    public async Task Logout_RevokesTheRefreshToken()
    {
        HttpClient client = CreateClient();
        TokenDto token = await client.LoginSuccessfullyAsync(AuthApiFactory.AdminEmail, AuthApiFactory.AdminPassword);

        HttpResponseMessage logout = await client.PostAsJsonAsync("/api/v1/Auth/logout", new { refreshToken = token.RefreshToken });

        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.RefreshAsync(token.RefreshToken)).StatusCode);
    }

    [Fact]
    public async Task ChangePassword_EndsAllSessions()
    {
        (HttpClient client, UserDto user) = await CreateUserClientAsync();

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/v1/User/change-password",
            new { oldPassword = ApiClientExtensions.DefaultPassword, newPassword = "N3wPassw0rd!" });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await GetMeAsync(client)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await CreateClient().LoginAsync(user.Email, ApiClientExtensions.DefaultPassword)).StatusCode);
        await CreateClient().LoginSuccessfullyAsync(user.Email, "N3wPassw0rd!");
    }

    [Fact]
    public async Task ChangePassword_RejectsAWeakNewPassword()
    {
        (HttpClient client, _) = await CreateUserClientAsync();

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/v1/User/change-password",
            new { oldPassword = ApiClientExtensions.DefaultPassword, newPassword = "weak" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Me_ReturnsEffectivePermissions()
    {
        CurrentUserDto admin = await (await GetMeAsync(await CreateAdminClientAsync())).ReadDataAsync<CurrentUserDto>();

        Assert.True(admin.IsSuperAdmin);
        Assert.Equal(Permissions.AllNames.Order(), admin.Permissions);

        (HttpClient client, _) = await CreateUserClientAsync(Permissions.Users.Read);
        CurrentUserDto user = await (await GetMeAsync(client)).ReadDataAsync<CurrentUserDto>();

        Assert.False(user.IsSuperAdmin);
        Assert.Equal([Permissions.Users.Read], user.Permissions);
        Assert.Single(user.Roles);
    }
}

public class RateLimitTests(RateLimitedAuthApiFactory factory) : IClassFixture<RateLimitedAuthApiFactory>
{
    [Fact]
    public async Task Login_IsRateLimited()
    {
        HttpClient client = factory.CreateClient();

        await client.LoginAsync("someone@test.local", "Whatever1!");
        await client.LoginAsync("someone@test.local", "Whatever1!");
        HttpResponseMessage limited = await client.LoginAsync("someone@test.local", "Whatever1!");

        Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
        Assert.Single(await limited.ReadErrorsAsync());
    }
}
