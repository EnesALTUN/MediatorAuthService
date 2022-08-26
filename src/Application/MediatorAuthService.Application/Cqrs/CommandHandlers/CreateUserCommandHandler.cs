using MediatorAuthService.Application.Cqrs.Commands.UserCommands;
using MediatorAuthService.Application.Wrappers;
using MediatorAuthService.Domain.Entities;
using MediatorAuthService.Infrastructure.UnitOfWork;
using MediatR;
using System.Net;

namespace MediatorAuthService.Application.Cqrs.CommandHandlers;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, ApiResponse<User>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<User>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        bool isExistUserByEmail = await _unitOfWork.GetRepository<User>().AnyAsync(x => x.Email.Equals(request.User.Email));

        if (isExistUserByEmail)
            return new ApiResponse<User>
            {
                Errors = new List<string> { "There is a record of the e-mail address." },
                IsSuccessful = false,
                StatusCode = (int)HttpStatusCode.BadRequest
            };

        await _unitOfWork.GetRepository<User>().AddAsync(request.User);
        await _unitOfWork.SaveChangesAsync();

        return new ApiResponse<User>
        {
            Data = request.User,
            IsSuccessful = true,
            StatusCode = (int)HttpStatusCode.Created,
            TotalItemCount = 1
        };
    }
}