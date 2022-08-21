using MediatorAuthService.Application.Cqrs.Queries.UserQueries;
using MediatorAuthService.Application.Wrappers;
using MediatorAuthService.Domain.Entities;
using MediatorAuthService.Infrastructure.Data.Context;
using MediatorAuthService.Infrastructure.UnitOfWork;
using MediatR;
using System.Net;

namespace MediatorAuthService.Application.Cqrs.QueryHandlers.UserQueryHandlers;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, ApiResponse<User>>
{
    private readonly IUnitOfWork<AppDbContext> _unitOfWork;

    public GetUserByIdQueryHandler(IUnitOfWork<AppDbContext> unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<User>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var userEntity = await _unitOfWork.GetRepository<User>().GetByIdAsync(request.Id);

        return new ApiResponse<User>
        {
            Data = userEntity,
            StatusCode = (int)HttpStatusCode.OK,
            IsSuccessful = true,
        };
    }
}