using MediatorAuthService.Application.Common.Authorization;
using MediatorAuthService.Application.Common.Persistence;
using MediatorAuthService.Application.Cqrs.Queries.UserQueries;
using MediatorAuthService.Application.Dtos.UserDtos;
using MediatorAuthService.Application.Exceptions;
using MediatorAuthService.Application.Wrappers;
using MediatorAuthService.Domain.Entities;
using MediatR;
using System.Net;

namespace MediatorAuthService.Application.Cqrs.QueryHandlers.UserQueryHandlers;

/// <summary>
/// Users can always read themselves; reading other users requires Users.Read.
/// </summary>
public class GetUserByIdQueryHandler(IUnitOfWork _unitOfWork, IAccessGuard _accessGuard) : IRequestHandler<GetUserByIdQuery, ApiResponse<UserDto>>
{
    public async Task<ApiResponse<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        await _accessGuard.EnsureSelfOrPermissionAsync(request.Id, Permissions.Users.Read, cancellationToken);

        UserDto existUser = await _unitOfWork.GetRepository<User>().GetByIdAsync(request.Id, UserDto.Projection, cancellationToken)
            ?? throw new NotFoundException("User is not found.");

        return new ApiResponse<UserDto>
        {
            Data = existUser,
            StatusCode = (int)HttpStatusCode.OK,
            IsSuccessful = true,
            TotalItemCount = 1
        };
    }
}
