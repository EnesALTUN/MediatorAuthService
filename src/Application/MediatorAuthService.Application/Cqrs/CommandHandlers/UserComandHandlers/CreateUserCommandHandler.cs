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

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, ApiResponse<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateUserCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

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