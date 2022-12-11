using MediatorAuthService.Application.Cqrs.Queries.AuthQueries;
using MediatorAuthService.Application.Dtos.AuthDtos;
using MediatorAuthService.Application.Dtos.ConfigurationDtos;
using MediatorAuthService.Application.Extensions;
using MediatorAuthService.Application.Wrappers;
using MediatorAuthService.Domain.Core.Extensions;
using MediatR;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;

namespace MediatorAuthService.Application.Cqrs.QueryHandlers.AuthQueryHandlers;

internal class GenerateTokenQueryHandler : IRequestHandler<GenerateTokenQuery, ApiResponse<TokenDto>>
{
    private readonly MediatorTokenOptions _tokenOption;

    public GenerateTokenQueryHandler(IOptions<MediatorTokenOptions> tokenOption)
    {
        _tokenOption = tokenOption.Value;
    }

    public Task<ApiResponse<TokenDto>> Handle(GenerateTokenQuery request, CancellationToken cancellationToken)
    {
        DateTime accessTokenExpiration = DateTime.Now.AddDays(_tokenOption.AccessTokenExpiration);
        DateTime refreshTokenExpiration = DateTime.Now.AddDays(_tokenOption.RefreshTokenExpiration);

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenOption.SecurityKey));

        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

        var jwtSecurityToken = new JwtSecurityToken(
            issuer: _tokenOption.Issuer,
            expires: accessTokenExpiration,
            notBefore: DateTime.Now,
            claims: UserClaimManager.GetClaims(request.User, _tokenOption.Audience),
            signingCredentials: signingCredentials);

        var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

        var tokenDto = new TokenDto
        {
            AccessToken = token,
            RefreshToken = HashingManager.HashValue(Guid.NewGuid().ToString()),
            AccessTokenExpire = accessTokenExpiration,
            RefreshTokenExpire = refreshTokenExpiration
        };

        return Task.FromResult(
            new ApiResponse<TokenDto>
            {
                Data = tokenDto,
                IsSuccessful = true,
                StatusCode = (int)HttpStatusCode.OK,
                TotalItemCount = 1
            });
    }
}