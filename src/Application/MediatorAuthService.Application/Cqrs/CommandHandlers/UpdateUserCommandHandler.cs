using MediatorAuthService.Application.Cqrs.Commands.UserCommands;
using MediatorAuthService.Application.Wrappers;
using MediatorAuthService.Domain.Entities;
using MediatorAuthService.Infrastructure.UnitOfWork;
using MediatR;
using System.Net;

namespace MediatorAuthService.Application.Cqrs.CommandHandlers;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, ApiResponse<User>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<User>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var existUser = await _unitOfWork.GetRepository<User>().GetByIdAsync(request.User.Id);

        if (existUser is null)
            return new ApiResponse<User>
            {
                Errors = new List<string> { "User is not found." },
                IsSuccessful = false,
                StatusCode = (int)HttpStatusCode.NotFound,
            };

        var updatedUser = _unitOfWork.GetRepository<User>().Update(request.User);
        await _unitOfWork.SaveChangesAsync();

        return new ApiResponse<User>
        {
            Data = updatedUser,
            IsSuccessful = true,
            StatusCode = (int)HttpStatusCode.OK,
            TotalItemCount = 1
        };
    }
}