using FluentValidation;
using MediatorAuthService.Application.Cqrs.Commands.UserCommands;
using MediatorAuthService.Application.Dtos.ResponseDtos;
using MediatorAuthService.Application.Extensions;
using MediatorAuthService.Application.Wrappers;
using MediatorAuthService.Domain.Core.Extensions;
using MediatorAuthService.Domain.Entities;
using MediatorAuthService.Infrastructure.UnitOfWork;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace MediatorAuthService.Application.Cqrs.CommandHandlers.UserComandHandlers;

/// <summary>
/// The ID information of the request owner is searched in the database with the active filter.
/// - If a matching record is found, the password sent from the request is compared with the passwords in the record in the database. 
///     - If the passwords do not match, an error is returned to the user. 
///     - If the passwords match, the new password from the request is assigned to the user.
/// - If no matching record is found, an error is returned.
/// </summary>
public class ChangePasswordUserCommandHandler : IRequestHandler<ChangePasswordUserCommand, ApiResponse<NoDataDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ChangePasswordUserCommandHandler(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ApiResponse<NoDataDto>> Handle(ChangePasswordUserCommand request, CancellationToken cancellationToken)
    {
        Guid userId = _httpContextAccessor.HttpContext!.User.Id();

        User? existUser = await _unitOfWork.GetRepository<User>()
            .Where(user => user.Id.Equals(userId) && user.IsActive)
            .SingleOrDefaultAsync(cancellationToken);

        if (existUser is null)
            throw new ValidationException("User email or password wrong.");

        if (!HashingManager.VerifyHashedValue(existUser.Password, request.OldPassword))
            throw new ValidationException("User email or password wrong.");

        string hashedNewPassword = HashingManager.HashValue(request.NewPassword);

        existUser.Password = hashedNewPassword;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ApiResponse<NoDataDto>
        {
            StatusCode = (int)HttpStatusCode.NoContent
        };
    }
}