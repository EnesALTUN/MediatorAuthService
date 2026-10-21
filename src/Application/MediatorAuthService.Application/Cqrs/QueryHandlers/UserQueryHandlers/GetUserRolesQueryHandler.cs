using MediatorAuthService.Application.Common.Authorization;
using MediatorAuthService.Application.Common.Persistence;
using MediatorAuthService.Application.Cqrs.Queries.UserQueries;
using MediatorAuthService.Application.Dtos.RoleDtos;
using MediatorAuthService.Application.Exceptions;
using MediatorAuthService.Application.Wrappers;
using MediatorAuthService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace MediatorAuthService.Application.Cqrs.QueryHandlers.UserQueryHandlers;

/// <summary>
/// Users can always read their own roles; reading the roles of others requires Users.Read.
/// </summary>
public class GetUserRolesQueryHandler(IUnitOfWork _unitOfWork, IAccessGuard _accessGuard) : IRequestHandler<GetUserRolesQuery, ApiResponse<List<RoleSummaryDto>>>
{
    public async Task<ApiResponse<List<RoleSummaryDto>>> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
    {
        await _accessGuard.EnsureSelfOrPermissionAsync(request.UserId, Permissions.Users.Read, cancellationToken);

        if (!await _unitOfWork.GetRepository<User>().AnyAsync(user => user.Id == request.UserId, cancellationToken))
            throw new NotFoundException("User is not found.");

        List<RoleSummaryDto> roles = await _unitOfWork.GetRepository<Role>()
            .Query()
            .Where(role => role.UserRoles.Any(userRole => userRole.UserId == request.UserId))
            .OrderBy(role => role.Name)
            .Select(RoleSummaryDto.Projection)
            .ToListAsync(cancellationToken);

        return new ApiResponse<List<RoleSummaryDto>>
        {
            Data = roles,
            IsSuccessful = true,
            StatusCode = (int)HttpStatusCode.OK,
            TotalItemCount = roles.Count
        };
    }
}
