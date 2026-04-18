using MediatorAuthService.Application.Common.Security;
using MediatorAuthService.Application.Dtos.ResponseDtos;
using MediatorAuthService.Application.Wrappers;
using MediatR;

namespace MediatorAuthService.Application.Cqrs.Commands.UserCommands;

public class ChangePasswordUserCommand(string oldPassword, string newPassword) : IRequest<ApiResponse<INoData>>
{
    [SensitiveData]
    public string OldPassword { get; set; } = oldPassword;

    [SensitiveData]
    public string NewPassword { get; set; } = newPassword;
}