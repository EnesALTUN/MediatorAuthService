using MediatorAuthService.Application.Cqrs.Queries.UserQueries;
using MediatorAuthService.Application.Wrappers;
using MediatorAuthService.Domain.Entities;
using MediatorAuthService.Infrastructure.Data.Context;
using MediatorAuthService.Infrastructure.UnitOfWork;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace MediatorAuthService.Application.Cqrs.QueryHandlers.UserQueryHandlers;

public class GetUserByEmailQueryHandler : IRequestHandler<GetUserByEmailQuery, ApiResponse<User>>
{
    private readonly IUnitOfWork<AppDbContext> _unitOfWork;

    public GetUserByEmailQueryHandler(IUnitOfWork<AppDbContext> unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<User>> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        var data = await _unitOfWork.GetRepository<User>().Where(x => x.Email.Equals(request.Email)).SingleOrDefaultAsync(cancellationToken);

        if (data is null)
            return new ApiResponse<User>
            {
                Errors = new() { "Kullanıcı Bulunamadı" },
                IsSuccessful = false,
                StatusCode = (int)HttpStatusCode.NotFound,
            };

        return new ApiResponse<User>
        {
            Data = data,
            IsSuccessful = true,
            StatusCode = (int)HttpStatusCode.OK,
            TotalItemCount = 1
        };
    }
}