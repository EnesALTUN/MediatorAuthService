using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatorAuthService.Application.Cqrs.Queries.UserQueries;
using MediatorAuthService.Application.Dtos.UserDtos;
using MediatorAuthService.Application.Wrappers;
using MediatorAuthService.Domain.Core.Pagination;
using MediatorAuthService.Domain.Entities;
using MediatorAuthService.Infrastructure.Data.Context;
using MediatorAuthService.Infrastructure.UnitOfWork;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace MediatorAuthService.Application.Cqrs.QueryHandlers.UserQueryHandlers;

public class GetUserAllQueryHandler(IUnitOfWork<AppDbContext> _unitOfWork, IMapper _mapper) : IRequestHandler<GetUserAllQuery, ApiResponse<List<UserDto>>>
{
    public async Task<ApiResponse<List<UserDto>>> Handle(GetUserAllQuery request, CancellationToken cancellationToken)
    {
        (IQueryable<User> userQuery, int totalCount) = _unitOfWork.GetRepository<User>()
            .GetAll(new PaginationParams
            {
                PageId = request.PageId,
                PageSize = request.PageSize,
                OrderKey = request.OrderKey,
                OrderType = request.OrderType
            });

        IQueryable<User> filteredData = ApplyFilter(userQuery, request.Name, request.Surname, request.Email, request.IsActive);

        List<UserDto> items = await filteredData
            .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new ApiResponse<List<UserDto>>
        {
            Data = items,
            StatusCode = (int)HttpStatusCode.OK,
            IsSuccessful = true,
            TotalItemCount = totalCount
        };
    }

    private static IQueryable<User> ApplyFilter(IQueryable<User> source, string? name, string? surname, string? email, bool? isActive)
    {
        if (!string.IsNullOrEmpty(name))
            source = source.Where(x => x.Name.Contains(name));

        if (!string.IsNullOrEmpty(surname))
            source = source.Where(x => x.Surname.Contains(surname));

        if (!string.IsNullOrEmpty(email))
            source = source.Where(x => x.Email.Contains(email));

        if (isActive is not null)
            source = source.Where(x => x.IsActive.Equals(isActive));

        return source;
    }
}