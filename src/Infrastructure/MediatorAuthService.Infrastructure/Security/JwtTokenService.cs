using MediatorAuthService.Application.Common.Abstractions;
using MediatorAuthService.Application.Common.Authorization;
using MediatorAuthService.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Buffers.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MediatorAuthService.Infrastructure.Security;

/// <summary>
/// Issues HMAC-SHA256 signed JWT access tokens and opaque refresh tokens.
/// Permissions are not embedded in the token; they are resolved per request so that changes apply immediately.
/// </summary>
internal sealed class JwtTokenService(IOptions<JwtOptions> options) : ITokenService
{
    private const int RefreshTokenBytes = 64;

    private readonly JwtOptions _options = options.Value;

    public AccessToken CreateAccessToken(User user)
    {
        DateTime now = DateTime.UtcNow;
        DateTime expires = now.AddMinutes(_options.AccessTokenExpirationMinutes);

        SigningCredentials signingCredentials = new(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecurityKey)),
            SecurityAlgorithms.HmacSha256);

        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.GivenName, user.Name),
            new(JwtRegisteredClaimNames.FamilyName, user.Surname),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(CustomClaimTypes.SecurityStamp, user.SecurityStamp.ToString()),
            .. _options.Audience.Select(audience => new Claim(JwtRegisteredClaimNames.Aud, audience)),
        ];

        JwtSecurityToken jwtSecurityToken = new(
            issuer: _options.Issuer,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: signingCredentials);

        return new AccessToken(new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken), expires);
    }

    public GeneratedRefreshToken CreateRefreshToken()
    {
        string token = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(RefreshTokenBytes));

        return new GeneratedRefreshToken(token, HashRefreshToken(token), DateTime.UtcNow.AddDays(_options.RefreshTokenExpirationDays));
    }

    // Refresh tokens are 512-bit random values, so a fast hash is sufficient; only the hash is stored.
    public string HashRefreshToken(string refreshToken) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
}
