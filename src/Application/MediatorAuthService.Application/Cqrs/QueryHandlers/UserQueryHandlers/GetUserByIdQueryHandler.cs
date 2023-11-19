using FluentValidation;
using MediatorAuthService.Application.Cqrs.Queries.UserQueries;
using MediatorAuthService.Application.Dtos.UserDtos;
using MediatorAuthService.Application.Wrappers;
using MediatorAuthService.Domain.Entities;
using MediatorAuthService.Infrastructure.Data.Context;
using MediatorAuthService.Infrastructure.UnitOfWork;
using MediatR;
using System.Net;

namespace MediatorAuthService.Application.Cqrs.QueryHandlers.UserQueryHandlers;

public class GetUserByIdQueryHandler(IUnitOfWork<AppDbContext> _unitOfWork) : IRequestHandler<GetUserByIdQuery, ApiResponse<UserDto>>
{
    public async Task<ApiResponse<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        UserDto? existUser = await _unitOfWork.GetRepository<User>().GetByIdWithProjectToAsync<UserDto>(request.Id, cancellationToken)
            ?? throw new ValidationException("User is not found.");

        return new ApiResponse<UserDto>
        {
            Data = existUser,
            StatusCode = (int)HttpStatusCode.OK,
            IsSuccessful = true,
            TotalItemCount = 1
        };
    }
}