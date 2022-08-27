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

public class GetUserAllQueryHandler : IRequestHandler<GetUserAllQuery, ApiResponse<List<UserDto>>>
{
    private readonly IUnitOfWork<AppDbContext> _unitOfWork;
    private readonly IMapper _mapper;

    public GetUserAllQueryHandler(IUnitOfWork<AppDbContext> unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<List<UserDto>>> Handle(GetUserAllQuery request, CancellationToken cancellationToken)
    {
        var resRepo = _unitOfWork.GetRepository<User>().GetAll(request.PaginationParams);

        var data = await resRepo.Item1.Select(x =>
            new UserDto
            {
                Id = x.Id,
                Email = x.Email,
                Name = x.Name,
                Surname = x.Surname,
                IsActive = x.IsActive,
            }).ToListAsync(cancellationToken);

        return new ApiResponse<List<UserDto>>
        {
            Data = _mapper.Map<List<UserDto>>(data),
            StatusCode = (int)HttpStatusCode.OK,
            IsSuccessful = true,
            TotalItemCount = resRepo.Item2
        };
    }
}