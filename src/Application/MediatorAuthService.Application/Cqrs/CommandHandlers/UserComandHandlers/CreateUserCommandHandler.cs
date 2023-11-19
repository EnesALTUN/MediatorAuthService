using AutoMapper;
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
/// Adds a new user to the system. 
/// If the sent email address is in the system, an error is returned.
/// </summary>
public class CreateUserCommandHandler(IUnitOfWork _unitOfWork, IMapper _mapper) : IRequestHandler<CreateUserCommand, ApiResponse<UserDto>>
{
    public async Task<ApiResponse<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        bool isExistUserByEmail = await _unitOfWork.GetRepository<User>().AnyAsync(x => x.Email.Equals(request.Email), cancellationToken);

        if (isExistUserByEmail)
            throw new BusinessException("There is a record of the e-mail address.");

        request.Password = HashingManager.HashValue(request.Password);

        User userEntity = _mapper.Map<User>(request);

        userEntity.RefreshToken = HashingManager.HashValue(Guid.NewGuid().ToString());

        await _unitOfWork.GetRepository<User>().AddAsync(userEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ApiResponse<UserDto>
        {
            Data = _mapper.Map<UserDto>(userEntity),
            IsSuccessful = true,
            StatusCode = (int)HttpStatusCode.Created,
            TotalItemCount = 1
        };
    }
}