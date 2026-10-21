using MediatorAuthService.Application.Common.Extensions;
using MediatorAuthService.Application.Common.Persistence;
using MediatorAuthService.Application.Cqrs.Queries.UserQueries;
using MediatorAuthService.Application.Dtos.UserDtos;
using MediatorAuthService.Application.Wrappers;
using MediatorAuthService.Domain.Entities;
using MediatR;
using System.Linq.Expressions;
using System.Net;

namespace MediatorAuthService.Application.Cqrs.QueryHandlers.UserQueryHandlers;

public class GetUserAllQueryHandler(IUnitOfWork _unitOfWork) : IRequestHandler<GetUserAllQuery, ApiResponse<List<UserDto>>>
{
    public async Task<ApiResponse<List<UserDto>>> Handle(GetUserAllQuery request, CancellationToken cancellationToken)
    {
        (List<UserDto> items, int totalCount) = await _unitOfWork.GetRepository<User>()
            .GetPagedAsync(
                request,
                UserDto.Projection,
                predicate: BuildPredicate(request),
                cancellationToken: cancellationToken
            );

        return new ApiResponse<List<UserDto>>
        {
            Data = items,
            StatusCode = (int)HttpStatusCode.OK,
            IsSuccessful = true,
            TotalItemCount = totalCount
        };
    }

    private static Expression<Func<User, bool>>? BuildPredicate(GetUserAllQuery request)
    {
        var predicates = new List<Expression<Func<User, bool>>>();

        if (!string.IsNullOrEmpty(request.Name))
            predicates.Add(x => x.Name.Contains(request.Name));

        if (!string.IsNullOrEmpty(request.Surname))
            predicates.Add(x => x.Surname.Contains(request.Surname));

        if (!string.IsNullOrEmpty(request.Email))
            predicates.Add(x => x.Email.Contains(request.Email));

        if (request.IsActive is not null)
            predicates.Add(x => x.IsActive == request.IsActive);

        if (predicates.Count == 0) return null;

        return predicates.Aggregate((a, b) => a.And(b));
    }
}
