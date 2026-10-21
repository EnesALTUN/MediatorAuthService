using MediatorAuthService.Application.Common.Authorization;
using MediatorAuthService.Application.Common.Persistence;
using MediatorAuthService.Application.Cqrs.Queries.AuthQueries;
using MediatorAuthService.Application.Dtos.AuthDtos;
using MediatorAuthService.Application.Dtos.RoleDtos;
using MediatorAuthService.Application.Dtos.UserDtos;
using MediatorAuthService.Application.Exceptions;
using MediatorAuthService.Application.Wrappers;
using MediatorAuthService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace MediatorAuthService.Application.Cqrs.QueryHandlers.AuthQueryHandlers;

/// <summary>
/// Returns the signed-in user together with roles and effective permissions.
/// </summary>
public class GetCurrentUserQueryHandler(IUnitOfWork _unitOfWork, IAccessGuard _accessGuard) : IRequestHandler<GetCurrentUserQuery, ApiResponse<CurrentUserDto>>
{
    public async Task<ApiResponse<CurrentUserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        UserAccessSnapshot access = await _accessGuard.GetCurrentAccessAsync(cancellationToken);

        UserDto user = await _unitOfWork.GetRepository<User>().GetByIdAsync(access.UserId, UserDto.Projection, cancellationToken)
            ?? throw new NotFoundException("User is not found.");

        List<RoleSummaryDto> roles = await _unitOfWork.GetRepository<Role>()
            .Where(role => role.UserRoles.Any(userRole => userRole.UserId == access.UserId))
            .OrderBy(role => role.Name)
            .Select(RoleSummaryDto.Projection)
            .ToListAsync(cancellationToken);

        IEnumerable<string> permissions = access.IsSuperAdmin ? Permissions.AllNames : access.Permissions;

        return new ApiResponse<CurrentUserDto>
        {
            Data = new CurrentUserDto
            {
                Id = user.Id,
                Name = user.Name,
                Surname = user.Surname,
                Email = user.Email,
                IsSuperAdmin = access.IsSuperAdmin,
                Roles = roles,
                Permissions = [.. permissions.Order()]
            },
            IsSuccessful = true,
            StatusCode = (int)HttpStatusCode.OK,
            TotalItemCount = 1
        };
    }
}
