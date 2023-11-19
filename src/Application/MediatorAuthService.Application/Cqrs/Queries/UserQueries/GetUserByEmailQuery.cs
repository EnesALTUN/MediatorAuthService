using MediatorAuthService.Application.Dtos.UserDtos;
using MediatorAuthService.Application.Wrappers;
using MediatR;

namespace MediatorAuthService.Application.Cqrs.Queries.UserQueries;

public class GetUserByEmailQuery(string email) : IRequest<ApiResponse<UserDto>>
{
    public string Email { get; private set; } = email;
}