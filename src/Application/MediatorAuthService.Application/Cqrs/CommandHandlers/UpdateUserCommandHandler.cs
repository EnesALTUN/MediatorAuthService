using AutoMapper;
using MediatorAuthService.Application.Cqrs.Commands.UserCommands;
using MediatorAuthService.Application.Dtos.UserDtos;
using MediatorAuthService.Application.Wrappers;
using MediatorAuthService.Domain.Entities;
using MediatorAuthService.Infrastructure.UnitOfWork;
using MediatR;
using System.Net;

namespace MediatorAuthService.Application.Cqrs.CommandHandlers;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, ApiResponse<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateUserCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var existUser = await _unitOfWork.GetRepository<User>().GetByIdAsync(request.Id);

        if (existUser is null)
            return new ApiResponse<UserDto>
            {
                Errors = new List<string> { "User is not found." },
                IsSuccessful = false,
                StatusCode = (int)HttpStatusCode.NotFound,
            };

        var mappedUser = _mapper.Map(request, existUser);

        _unitOfWork.GetRepository<User>().Update(mappedUser);
        await _unitOfWork.SaveChangesAsync();

        return new ApiResponse<UserDto>
        {
            Data = _mapper.Map<UserDto>(mappedUser),
            IsSuccessful = true,
            StatusCode = (int)HttpStatusCode.OK,
            TotalItemCount = 1
        };
    }
}