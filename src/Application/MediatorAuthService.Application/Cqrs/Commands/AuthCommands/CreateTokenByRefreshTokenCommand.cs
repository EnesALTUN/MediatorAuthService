using MediatorAuthService.Application.Common.Security;
using MediatorAuthService.Application.Dtos.AuthDtos;
using MediatorAuthService.Application.Wrappers;
using MediatR;

namespace MediatorAuthService.Application.Cqrs.Commands.AuthCommands;

public class CreateTokenByRefreshTokenCommand(string refreshToken) : IRequest<ApiResponse<TokenDto>>
{
    [SensitiveData]
    public string RefreshToken { get; set; } = refreshToken;
}
