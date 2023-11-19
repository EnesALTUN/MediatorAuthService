using MediatorAuthService.Application.Dtos.AuthDtos;
using MediatorAuthService.Application.Wrappers;
using MediatR;

namespace MediatorAuthService.Application.Cqrs.Commands.AuthCommands;

public class CreateTokenByRefreshTokenCommand(string refreshToken) : IRequest<ApiResponse<TokenDto>>
{
    public string RefreshToken { get; set; } = refreshToken;
}