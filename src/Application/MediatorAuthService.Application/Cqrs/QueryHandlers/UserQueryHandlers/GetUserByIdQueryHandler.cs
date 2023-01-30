using AutoMapper;
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

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, ApiResponse<UserDto>>
{
    private readonly IUnitOfWork<AppDbContext> _unitOfWork;

    public GetUserByIdQueryHandler(IUnitOfWork<AppDbContext> unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var existRecord = await _unitOfWork.GetRepository<User>().GetByIdWithProjectToAsync<UserDto>(request.Id, cancellationToken);

        if (existRecord is null)
            throw new ValidationException("User is not found.");

        return new ApiResponse<UserDto>
        {
            Data = existRecord,
            StatusCode = (int)HttpStatusCode.OK,
            IsSuccessful = true,
            TotalItemCount = 1
        };
    }
}