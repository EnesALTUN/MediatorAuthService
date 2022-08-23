using MediatorAuthService.Application.Wrappers;
using MediatorAuthService.Domain.Core.Pagination;
using MediatorAuthService.Domain.Entities;
using MediatR;

namespace MediatorAuthService.Application.Cqrs.Queries.UserQueries;

public class GetUserAllQuery : IRequest<ApiResponse<List<User>>>
{
    public PaginationParams PaginationParams { get; set; }

    public GetUserAllQuery(PaginationParams request)
    {
        PaginationParams = request;
    }
}