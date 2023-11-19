using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using MediatorAuthService.Application.Cqrs.Queries.UserQueries;
using MediatorAuthService.Application.Dtos.UserDtos;
using MediatorAuthService.Application.Wrappers;
using MediatorAuthService.Domain.Entities;
using MediatorAuthService.Infrastructure.Data.Context;
using MediatorAuthService.Infrastructure.UnitOfWork;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace MediatorAuthService.Application.Cqrs.QueryHandlers.UserQueryHandlers;

public class GetUserByEmailQueryHandler(IUnitOfWork<AppDbContext> _unitOfWork, IMapper _mapper) : IRequestHandler<GetUserByEmailQuery, ApiResponse<UserDto>>
{
    public async Task<ApiResponse<UserDto>> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        UserDto? user = await _unitOfWork.GetRepository<User>()
            .Where(x => x.Email.Equals(request.Email))
            .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(cancellationToken) 
          ?? throw new ValidationException("User is not found.");

        return new ApiResponse<UserDto>
        {
            Data = user,
            IsSuccessful = true,
            StatusCode = (int)HttpStatusCode.OK,
            TotalItemCount = 1
        };
    }
}