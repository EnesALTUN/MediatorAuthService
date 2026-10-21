using MediatorAuthService.Application.Common.Extensions;
using MediatorAuthService.Application.Common.Persistence;
using MediatorAuthService.Application.Cqrs.Commands.UserCommands;
using MediatorAuthService.Application.Dtos.UserDtos;
using MediatorAuthService.Application.Exceptions;
using MediatorAuthService.Application.Wrappers;
using MediatorAuthService.Domain.Core.Extensions;
using MediatorAuthService.Domain.Entities;
using MediatR;
using System.Net;

namespace MediatorAuthService.Application.Cqrs.CommandHandlers.UserComandHandlers;

/// <summary>
/// Adds a new user to the system.
/// If the sent email address belongs to an existing (not deleted) user, a conflict is returned.
/// </summary>
public class CreateUserCommandHandler(IUnitOfWork _unitOfWork) : IRequestHandler<CreateUserCommand, ApiResponse<UserDto>>
{
    public async Task<ApiResponse<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        string email = request.Email.NormalizeEmail();

        bool isExistUserByEmail = await _unitOfWork.GetRepository<User>().AnyAsync(user => user.Email == email, cancellationToken);

        if (isExistUserByEmail)
            throw new ConflictException("There is a record of the e-mail address.");

        User userEntity = new()
        {
            Name = request.Name.Trim(),
            Surname = request.Surname.Trim(),
            Email = email,
            Password = HashingManager.HashValue(request.Password)
        };

        await _unitOfWork.GetRepository<User>().AddAsync(userEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ApiResponse<UserDto>
        {
            Data = UserDto.FromEntity(userEntity),
            IsSuccessful = true,
            StatusCode = (int)HttpStatusCode.Created,
            TotalItemCount = 1
        };
    }
}
