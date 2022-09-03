using MediatorAuthService.Application.Dtos.AuthDtos;
using MediatorAuthService.Application.Wrappers;
using MediatR;

namespace MediatorAuthService.Application.Cqrs.Commands.AuthCommands;

public class CreateTokenByRefreshTokenCommand : IRequest<ApiResponse<TokenDto>>
{
    public string RefreshToken { get; set; }

    public CreateTokenByRefreshTokenCommand(string refreshToken)
    {
        RefreshToken = refreshToken;
    }
}