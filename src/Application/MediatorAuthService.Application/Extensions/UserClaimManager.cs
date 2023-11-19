using MediatorAuthService.Application.Dtos.UserDtos;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MediatorAuthService.Application.Extensions;

internal static class UserClaimManager
{
    public static IEnumerable<Claim> GetClaims(UserDto user, List<string> audiences)
    {
        List<Claim> userList = [
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Surname, user.Surname),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                .. audiences.Select(audience => new Claim(JwtRegisteredClaimNames.Aud, audience)),
            ];

        return userList;
    }
}