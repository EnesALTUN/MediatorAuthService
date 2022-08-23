using MediatorAuthService.Application.Cqrs.Queries.UserQueries;
using MediatorAuthService.Application.Wrappers;
using MediatorAuthService.Domain.Entities;
using MediatorAuthService.Infrastructure.Data.Context;
using MediatorAuthService.Infrastructure.UnitOfWork;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace MediatorAuthService.Application.Cqrs.QueryHandlers.UserQueryHandlers;

public class GetUserAllQueryHandler : IRequestHandler<GetUserAllQuery, ApiResponse<List<User>>>
{
    private readonly IUnitOfWork<AppDbContext> _unitOfWork;

    public GetUserAllQueryHandler(IUnitOfWork<AppDbContext> unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<List<User>>> Handle(GetUserAllQuery request, CancellationToken cancellationToken)
    {
        var resRepo = _unitOfWork.GetRepository<User>().GetAll(request.PaginationParams);

        var data = await resRepo.Item1.ToListAsync(cancellationToken);

        return new ApiResponse<List<User>>
        {
            Data = data,
            StatusCode = (int)HttpStatusCode.OK,
            IsSuccessful = true,
            TotalItemCount = resRepo.Item2
        };
    }
}