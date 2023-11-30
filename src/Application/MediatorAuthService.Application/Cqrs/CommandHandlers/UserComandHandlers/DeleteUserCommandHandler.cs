using FluentValidation;
using MediatorAuthService.Application.Cqrs.Commands.UserCommands;
using MediatorAuthService.Application.Dtos.ResponseDtos;
using MediatorAuthService.Application.Wrappers;
using MediatorAuthService.Domain.Entities;
using MediatorAuthService.Infrastructure.UnitOfWork;
using MediatR;
using System.Net;

namespace MediatorAuthService.Application.Cqrs.CommandHandlers.UserComandHandlers;

/// <summary>
/// Deletes the user in the system.
///     - If the sent user ID exists in the system, the user is deleted.
///     - If the sent user ID is not found in the system, an error is returned.
/// </summary>
public class DeleteUserCommandHandler(IUnitOfWork _unitOfWork) : IRequestHandler<DeleteUserCommand, ApiResponse<INoData>>
{
    public async Task<ApiResponse<INoData>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        User? existUser = await _unitOfWork.GetRepository<User>().GetByIdAsync(request.Id, cancellationToken) 
            ?? throw new ValidationException("User is not found.");

        _unitOfWork.GetRepository<User>().Remove(existUser);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ApiResponse<INoData>
        {
            StatusCode = (int)HttpStatusCode.NoContent,
            IsSuccessful = true,
        };
    }
}