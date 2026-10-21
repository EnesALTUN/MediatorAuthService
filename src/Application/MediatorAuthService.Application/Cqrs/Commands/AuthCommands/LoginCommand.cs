using MediatorAuthService.Application.Common.Security;
using MediatorAuthService.Application.Dtos.AuthDtos;
using MediatorAuthService.Application.Wrappers;
using MediatR;

namespace MediatorAuthService.Application.Cqrs.Commands.AuthCommands;

public class LoginCommand : IRequest<ApiResponse<TokenDto>>
{
    [PersonalData]
    public required string Email { get; set; }

    [SensitiveData]
    public required string Password { get; set; }
}
