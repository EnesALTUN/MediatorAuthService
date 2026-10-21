using MediatorAuthService.Application.Dtos.AuthDtos;
using MediatorAuthService.Application.Wrappers;
using MediatR;

namespace MediatorAuthService.Application.Cqrs.Queries.AuthQueries;

public class GetCurrentUserQuery : IRequest<ApiResponse<CurrentUserDto>>
{
}
