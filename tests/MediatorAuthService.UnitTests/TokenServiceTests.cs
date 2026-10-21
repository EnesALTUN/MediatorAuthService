using MediatorAuthService.Application.Common.Abstractions;
using MediatorAuthService.Application.Common.Authorization;
using MediatorAuthService.Domain.Entities;
using MediatorAuthService.Infrastructure.Security;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;

namespace MediatorAuthService.UnitTests;

public class TokenServiceTests
{
    private static JwtOptions CreateOptions() => new()
    {
        Audience = ["aud-1", "aud-2"],
        Issuer = "issuer",
        SecurityKey = new string('k', 32),
        AccessTokenExpirationMinutes = 15,
        RefreshTokenExpirationDays = 30
    };

    [Fact]
    public void CreateAccessToken_ContainsIdentityAndSecurityStamp_ButNoPermissions()
    {
        User user = new() { Id = Guid.NewGuid(), Name = "John", Surname = "Doe", Email = "john@example.com", Password = "-" };
        JwtTokenService service = new(Options.Create(CreateOptions()));

        AccessToken accessToken = service.CreateAccessToken(user);
        JwtSecurityToken token = new JwtSecurityTokenHandler().ReadJwtToken(accessToken.Token);

        Assert.DoesNotContain("Bearer", accessToken.Token);
        Assert.Equal(user.Id.ToString(), token.Subject);
        Assert.Equal(user.SecurityStamp.ToString(), token.Claims.Single(claim => claim.Type == CustomClaimTypes.SecurityStamp).Value);
        Assert.Equal(["aud-1", "aud-2"], token.Audiences);
        Assert.InRange(accessToken.ExpiresAt, DateTime.UtcNow.AddMinutes(14), DateTime.UtcNow.AddMinutes(16));
    }

    [Fact]
    public void CreateRefreshToken_StoresOnlyAHash()
    {
        JwtTokenService service = new(Options.Create(CreateOptions()));

        GeneratedRefreshToken first = service.CreateRefreshToken();
        GeneratedRefreshToken second = service.CreateRefreshToken();

        Assert.NotEqual(first.Token, second.Token);
        Assert.NotEqual(first.Token, first.TokenHash);
        Assert.Equal(first.TokenHash, service.HashRefreshToken(first.Token));
    }

    [Fact]
    public void JwtOptions_RejectsShortKeysAndMissingValues()
    {
        JwtOptions options = CreateOptions();
        Assert.Empty(options.Validate());

        options.SecurityKey = "short";
        options.Audience = [];
        options.AccessTokenExpirationMinutes = 0;

        Assert.Equal(3, options.Validate().Count());
    }
}
