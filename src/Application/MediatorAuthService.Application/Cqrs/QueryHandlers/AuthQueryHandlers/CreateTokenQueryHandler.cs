using AutoMapper;
using FluentValidation;
using MediatorAuthService.Application.Cqrs.Queries.AuthQueries;
using MediatorAuthService.Application.Dtos.AuthDtos;
using MediatorAuthService.Application.Dtos.UserDtos;
using MediatorAuthService.Application.Wrappers;
using MediatorAuthService.Domain.Core.Extensions;
using MediatorAuthService.Domain.Entities;
using MediatorAuthService.Infrastructure.UnitOfWork;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace MediatorAuthService.Application.Cqrs.QueryHandlers.AuthQueryHandlers;

public class CreateTokenQueryHandler(IUnitOfWork _unitOfWork, IMediator _mediator, IMapper _mapper) : IRequestHandler<CreateTokenQuery, ApiResponse<TokenDto>>
{
    public async Task<ApiResponse<TokenDto>> Handle(CreateTokenQuery request, CancellationToken cancellationToken)
    {
        User? existUser = await _unitOfWork.GetRepository<User>()
            .Where(x => x.Email.Equals(request.Email))
            .SingleOrDefaultAsync(cancellationToken) 
          ?? throw new ValidationException("User is not found.");

        if (!existUser.IsActive)
            throw new ValidationException("The user is inactive in the system.");

        if (!HashingManager.VerifyHashedValue(existUser.Password, request.Password))
            throw new ValidationException("User is not found.");

        UserDto userDto = _mapper.Map<UserDto>(existUser);

        ApiResponse<TokenDto>? generatedToken = await _mediator.Send(new GenerateTokenQuery(userDto), cancellationToken);

        return new ApiResponse<TokenDto>
        {
            Data = generatedToken.Data,
            IsSuccessful = true,
            StatusCode = (int)HttpStatusCode.OK,
            TotalItemCount = 1
        };
    }
}