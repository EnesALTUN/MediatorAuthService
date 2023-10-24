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
public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, ApiResponse<NoDataDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteUserCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<NoDataDto>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        User? existUser = await _unitOfWork.GetRepository<User>().GetByIdAsync(request.Id, cancellationToken);

        if (existUser is null)
            throw new ValidationException("User is not found.");

        _unitOfWork.GetRepository<User>().Remove(existUser);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ApiResponse<NoDataDto>
        {
            StatusCode = (int)HttpStatusCode.NoContent,
            IsSuccessful = true,
        };
    }
}