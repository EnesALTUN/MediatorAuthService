using MediatorAuthService.Application.Common.Security;
using MediatorAuthService.Application.Dtos.UserDtos;
using MediatorAuthService.Application.Wrappers;
using MediatorAuthService.Domain.Core.Pagination;
using MediatR;

namespace MediatorAuthService.Application.Cqrs.Queries.UserQueries;

public class GetUserAllQuery : PaginationParams, IRequest<ApiResponse<List<UserDto>>>
{
    public string? Name { get; set; }

    public string? Surname { get; set; }

    [PersonalData]
    public string? Email { get; set; }

    public bool? IsActive { get; set; }
}
