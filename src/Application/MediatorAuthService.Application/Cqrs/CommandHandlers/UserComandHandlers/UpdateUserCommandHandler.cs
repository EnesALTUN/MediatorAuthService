using AutoMapper;
using FluentValidation;
using MediatorAuthService.Application.Cqrs.Commands.UserCommands;
using MediatorAuthService.Application.Dtos.UserDtos;
using MediatorAuthService.Application.Exceptions;
using MediatorAuthService.Application.Wrappers;
using MediatorAuthService.Domain.Core.Extensions;
using MediatorAuthService.Domain.Entities;
using MediatorAuthService.Infrastructure.UnitOfWork;
using MediatR;
using System.Net;

namespace MediatorAuthService.Application.Cqrs.CommandHandlers.UserComandHandlers;

/// <summary>
/// The ID information of the request owner is searched in the database.
/// - If a matching record is found, first the user's email address in the system is compared with the email address received from the request.
///     In this comparison, if the user's email address wants to be changed, it is checked that the user has entered an email address that does not exist in the system.
/// - If the user's old password and full password are sent, the old password in the request is compared with the user password in the system.
///     If the comparison result is successful, the new password from the request is assigned to the user; if unsuccessful, an error is returned to the user.
/// </summary>
public class UpdateUserCommandHandler(IUnitOfWork _unitOfWork, IMapper _mapper) : IRequestHandler<UpdateUserCommand, ApiResponse<UserDto>>
{
    public async Task<ApiResponse<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        User? existUser = await _unitOfWork.GetRepository<User>().GetByIdAsync(request.Id, cancellationToken) 
            ?? throw new ValidationException("User is not found.");

        if (!await ExistingEMailControlInEMailExchange(existUser.Email, request.Email, cancellationToken))
            throw new BusinessException("The entered e-mail address is used.");

        request.Password = string.IsNullOrEmpty(request.OldPassword) || string.IsNullOrEmpty(request.Password)
            ? existUser.Password
            : ChangePasswordProcess(existUser.Password, request.OldPassword, request.Password);

        _mapper.Map(request, existUser);

        _unitOfWork.GetRepository<User>().Update(existUser);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ApiResponse<UserDto>
        {
            Data = _mapper.Map<UserDto>(existUser),
            IsSuccessful = true,
            StatusCode = (int)HttpStatusCode.OK,
            TotalItemCount = 1
        };
    }

    private async Task<bool> ExistingEMailControlInEMailExchange(string oldEmail, string newEmail, CancellationToken cancellationToken)
    {
        if (oldEmail.Equals(newEmail))
            return true;

        return !await _unitOfWork.GetRepository<User>().AnyAsync(user => user.Email.Equals(newEmail), cancellationToken);
    }

    private static string ChangePasswordProcess(string orjPassword, string oldPassword, string newPassword)
    {
        return HashingManager.VerifyHashedValue(orjPassword, oldPassword)
                ? HashingManager.HashValue(newPassword)
                : throw new ValidationException("Your password does not match.");
    }
}