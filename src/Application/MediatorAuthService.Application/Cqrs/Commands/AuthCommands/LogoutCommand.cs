using MediatorAuthService.Application.Common.Security;
using MediatorAuthService.Application.Dtos.ResponseDtos;
using MediatorAuthService.Application.Wrappers;
using MediatR;

namespace MediatorAuthService.Application.Cqrs.Commands.AuthCommands;

public class LogoutCommand(string refreshToken) : IRequest<ApiResponse<NoData>>
{
    [SensitiveData]
    public string RefreshToken { get; set; } = refreshToken;
}
