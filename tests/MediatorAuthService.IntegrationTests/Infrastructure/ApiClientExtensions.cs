using MediatorAuthService.Application.Dtos.AuthDtos;
using MediatorAuthService.Application.Dtos.RoleDtos;
using MediatorAuthService.Application.Dtos.UserDtos;
using MediatorAuthService.Application.Wrappers;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MediatorAuthService.IntegrationTests.Infrastructure;

public static class ApiClientExtensions
{
    public const string DefaultPassword = "Passw0rd!";

    public static string UniqueEmail(string prefix = "user") => $"{prefix}-{Guid.NewGuid():N}@test.local";

    public static HttpClient WithToken(this HttpClient client, string accessToken)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }

    public static async Task<TData> ReadDataAsync<TData>(this HttpResponseMessage response)
    {
        ApiResponse<TData>? body = await response.Content.ReadFromJsonAsync<ApiResponse<TData>>();
        Assert.NotNull(body);
        Assert.NotNull(body.Data);
        return body.Data;
    }

    public static async Task<List<string>> ReadErrorsAsync(this HttpResponseMessage response)
    {
        ApiResponse<object>? body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.NotNull(body);
        Assert.False(body.IsSuccessful);
        Assert.Equal((int)response.StatusCode, body.StatusCode);
        Assert.False(string.IsNullOrEmpty(body.TraceId));
        return body.Errors ?? [];
    }

    public static Task<HttpResponseMessage> LoginAsync(this HttpClient client, string email, string password) =>
        client.PostAsJsonAsync("/api/v1/Auth/login", new { email, password });

    public static async Task<TokenDto> LoginSuccessfullyAsync(this HttpClient client, string email, string password)
    {
        HttpResponseMessage response = await client.LoginAsync(email, password);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.ReadDataAsync<TokenDto>();
    }

    public static Task<HttpResponseMessage> RefreshAsync(this HttpClient client, string refreshToken) =>
        client.PostAsJsonAsync("/api/v1/Auth/refresh-token", new { refreshToken });

    public static async Task<UserDto> CreateUserAsync(this HttpClient adminClient, string email, string password = DefaultPassword)
    {
        HttpResponseMessage response = await adminClient.PostAsJsonAsync("/api/v1/User", new { name = "Test", surname = "User", email, password });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await response.ReadDataAsync<UserDto>();
    }

    public static async Task<RoleDto> CreateRoleAsync(this HttpClient client, params string[] permissions)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/v1/Role", new { name = $"role-{Guid.NewGuid():N}", permissions });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await response.ReadDataAsync<RoleDto>();
    }

    public static Task<HttpResponseMessage> SetRolesAsync(this HttpClient client, Guid userId, params Guid[] roleIds) =>
        client.PutAsJsonAsync($"/api/v1/User/{userId}/roles", new { roleIds });
}
