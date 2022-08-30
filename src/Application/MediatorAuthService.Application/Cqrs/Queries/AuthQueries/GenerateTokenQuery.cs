using MediatorAuthService.Application.Dtos.AuthDtos;
using MediatorAuthService.Application.Dtos.UserDtos;
using MediatorAuthService.Application.Wrappers;
using MediatR;

namespace MediatorAuthService.Application.Cqrs.Queries.AuthQueries;

internal class GenerateTokenQuery : IRequest<ApiResponse<TokenDto>>
{
    public UserDto User { get; set; }

    public GenerateTokenQuery(UserDto user)
    {
        User = user;
    }
}