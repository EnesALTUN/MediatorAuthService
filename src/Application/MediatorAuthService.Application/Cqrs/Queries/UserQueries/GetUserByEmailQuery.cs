using MediatorAuthService.Application.Dtos.UserDtos;
using MediatorAuthService.Application.Wrappers;
using MediatR;

namespace MediatorAuthService.Application.Cqrs.Queries.UserQueries;

public class GetUserByEmailQuery : IRequest<ApiResponse<UserDto>>
{
    public string Email { get; private set; }

    public GetUserByEmailQuery(string email)
    {
        Email = email;
    }
}