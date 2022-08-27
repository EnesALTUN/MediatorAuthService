using AutoMapper;
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

public class GetUserByEmailQueryHandler : IRequestHandler<GetUserByEmailQuery, ApiResponse<UserDto>>
{
    private readonly IUnitOfWork<AppDbContext> _unitOfWork;
    private readonly IMapper _mapper;

    public GetUserByEmailQueryHandler(IUnitOfWork<AppDbContext> unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<UserDto>> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        var data = await _unitOfWork.GetRepository<User>()
            .Where(x => x.Email.Equals(request.Email))
            .Select(x =>
                new UserDto
                {
                    Id = x.Id,
                    Email = x.Email,
                    Name = x.Name,
                    Surname = x.Surname,
                    IsActive = x.IsActive,
                })
            .SingleOrDefaultAsync(cancellationToken);

        if (data is null)
            return new ApiResponse<UserDto>
            {
                Errors = new() { "Kullanıcı Bulunamadı" },
                IsSuccessful = false,
                StatusCode = (int)HttpStatusCode.NotFound,
            };

        return new ApiResponse<UserDto>
        {
            Data = _mapper.Map<UserDto>(data),
            IsSuccessful = true,
            StatusCode = (int)HttpStatusCode.OK,
            TotalItemCount = 1
        };
    }
}