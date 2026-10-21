using MediatorAuthService.Application.Common.Extensions;
using MediatorAuthService.Application.Common.Persistence;
using MediatorAuthService.Application.Cqrs.Queries.UserQueries;
using MediatorAuthService.Application.Dtos.UserDtos;
using MediatorAuthService.Application.Exceptions;
using MediatorAuthService.Application.Wrappers;
using MediatorAuthService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace MediatorAuthService.Application.Cqrs.QueryHandlers.UserQueryHandlers;

public class GetUserByEmailQueryHandler(IUnitOfWork _unitOfWork) : IRequestHandler<GetUserByEmailQuery, ApiResponse<UserDto>>
{
    public async Task<ApiResponse<UserDto>> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        string email = request.Email.NormalizeEmail();

        UserDto user = await _unitOfWork.GetRepository<User>()
            .Where(x => x.Email == email)
            .Select(UserDto.Projection)
            .SingleOrDefaultAsync(cancellationToken)
          ?? throw new NotFoundException("User is not found.");

        return new ApiResponse<UserDto>
        {
            Data = user,
            IsSuccessful = true,
            StatusCode = (int)HttpStatusCode.OK,
            TotalItemCount = 1
        };
    }
}
