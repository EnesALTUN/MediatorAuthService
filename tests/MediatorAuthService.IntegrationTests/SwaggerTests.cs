using MediatorAuthService.IntegrationTests.Infrastructure;
using System.Net;

namespace MediatorAuthService.IntegrationTests;

public class SwaggerTests(DevelopmentAuthApiFactory factory) : IClassFixture<DevelopmentAuthApiFactory>
{
    [Fact]
    public async Task SwaggerDocument_IsGeneratedForVersionOne()
    {
        HttpResponseMessage response = await factory.CreateClient().GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string document = await response.Content.ReadAsStringAsync();
        Assert.Contains("/api/v1/Role", document);
        Assert.Contains("/api/v1/Auth/refresh-token", document);
    }
}
