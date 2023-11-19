using MediatorAuthService.Application.Dtos.UserDtos;
using MediatorAuthService.Application.Wrappers;
using MediatR;

namespace MediatorAuthService.Application.Cqrs.Queries.UserQueries;

public class GetUserByIdQuery(Guid id) : IRequest<ApiResponse<UserDto>>
{
    public Guid Id { get; private set; } = id;
}