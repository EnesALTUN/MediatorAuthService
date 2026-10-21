using MediatorAuthService.Application.Dtos.RoleDtos;
using MediatorAuthService.Application.Wrappers;
using MediatR;

namespace MediatorAuthService.Application.Cqrs.Queries.UserQueries;

public class GetUserRolesQuery(Guid userId) : IRequest<ApiResponse<List<RoleSummaryDto>>>
{
    public Guid UserId { get; private set; } = userId;
}
