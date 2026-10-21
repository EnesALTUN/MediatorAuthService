using MediatorAuthService.Application.Dtos.UserDtos;
using MediatorAuthService.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace MediatorAuthService.IntegrationTests;

public class UserManagementTests(AuthApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CreateUser_WithAnExistingEmail_ReturnsConflict()
    {
        HttpClient adminClient = await CreateAdminClientAsync();
        string email = ApiClientExtensions.UniqueEmail();
        await adminClient.CreateUserAsync(email);

        HttpResponseMessage response = await adminClient.PostAsJsonAsync("/api/v1/User",
            new { name = "Test", surname = "User", email = email.ToUpperInvariant(), password = ApiClientExtensions.DefaultPassword });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task DeletedUsers_AreHidden_AndTheirEmailCanBeReused()
    {
        HttpClient adminClient = await CreateAdminClientAsync();
        string email = ApiClientExtensions.UniqueEmail();
        UserDto user = await adminClient.CreateUserAsync(email);

        Assert.Equal(HttpStatusCode.NoContent, (await adminClient.DeleteAsync($"/api/v1/User/{user.Id}")).StatusCode);

        Assert.Equal(HttpStatusCode.NotFound, (await adminClient.GetAsync($"/api/v1/User/{user.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await adminClient.DeleteAsync($"/api/v1/User/{user.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await CreateClient().LoginAsync(email, ApiClientExtensions.DefaultPassword)).StatusCode);

        UserDto recreated = await adminClient.CreateUserAsync(email);
        Assert.NotEqual(user.Id, recreated.Id);
    }

    [Fact]
    public async Task Users_CannotDeleteThemselves()
    {
        HttpClient adminClient = await CreateAdminClientAsync();
        UserDto admin = await (await adminClient.GetAsync($"/api/v1/User/{AuthApiFactory.AdminEmail}")).ReadDataAsync<UserDto>();

        Assert.Equal(HttpStatusCode.Forbidden, (await adminClient.DeleteAsync($"/api/v1/User/{admin.Id}")).StatusCode);
    }

    [Theory]
    [InlineData("?pageSize=1000")]
    [InlineData("?orderKey=Password")]
    [InlineData("?orderKey=DoesNotExist")]
    [InlineData("?orderType=sideways")]
    public async Task GetAllUsers_RejectsInvalidPagingAndSorting(string query)
    {
        HttpResponseMessage response = await (await CreateAdminClientAsync()).GetAsync($"/api/v1/User{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotEmpty(await response.ReadErrorsAsync());
    }

    [Fact]
    public async Task GetAllUsers_PagesAndFilters()
    {
        HttpClient adminClient = await CreateAdminClientAsync();
        string marker = Guid.NewGuid().ToString("N");
        await adminClient.CreateUserAsync($"{marker}-1@test.local");
        await adminClient.CreateUserAsync($"{marker}-2@test.local");

        HttpResponseMessage response = await adminClient.GetAsync($"/api/v1/User?email={marker}&pageSize=1&orderKey=email&orderType=descending");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        List<UserDto> users = await response.ReadDataAsync<List<UserDto>>();
        Assert.Equal($"{marker}-2@test.local", Assert.Single(users).Email);
    }

    [Fact]
    public async Task MalformedJson_ReturnsBadRequestInTheApiEnvelope()
    {
        HttpClient adminClient = await CreateAdminClientAsync();

        HttpResponseMessage response = await adminClient.PostAsync("/api/v1/User", new StringContent("{ not json", Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotEmpty(await response.ReadErrorsAsync());
    }

    [Fact]
    public async Task HealthEndpoints_AreAnonymous()
    {
        HttpClient client = CreateClient();

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health/live")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health/ready")).StatusCode);
    }
}
