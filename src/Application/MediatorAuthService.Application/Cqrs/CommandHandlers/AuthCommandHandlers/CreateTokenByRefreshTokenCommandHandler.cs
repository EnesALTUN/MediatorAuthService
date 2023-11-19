using AutoMapper;
using FluentValidation;
using MediatorAuthService.Application.Cqrs.Commands.AuthCommands;
using MediatorAuthService.Application.Cqrs.Queries.AuthQueries;
using MediatorAuthService.Application.Dtos.AuthDtos;
using MediatorAuthService.Application.Dtos.UserDtos;
using MediatorAuthService.Application.Exceptions;
using MediatorAuthService.Application.Extensions;
using MediatorAuthService.Application.Wrappers;
using MediatorAuthService.Domain.Entities;
using MediatorAuthService.Infrastructure.UnitOfWork;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace MediatorAuthService.Application.Cqrs.CommandHandlers.AuthCommandHandlers;

/// <summary>
/// It queries the database for the record matching the request owner's ID information and the sent refresh token information.
/// If a matching record is found, new token information is generated and returned and the user's refresh token information is updated.
/// If no matching record is found, an error is returned.
/// </summary>
public class CreateTokenByRefreshTokenCommandHandler(IUnitOfWork _unitOfWork, IMapper _mapper, IMediator _mediator, IHttpContextAccessor _httpContextAccessor) : IRequestHandler<CreateTokenByRefreshTokenCommand, ApiResponse<TokenDto>>
{
    public async Task<ApiResponse<TokenDto>> Handle(CreateTokenByRefreshTokenCommand request, CancellationToken cancellationToken)
    {
        Guid userId = _httpContextAccessor.HttpContext!.User.Id();

        User? existUser = await _unitOfWork.GetRepository<User>()
            .Where(user => user.Id.Equals(userId) && user.RefreshToken.Equals(request.RefreshToken) && user.IsActive)
            .SingleOrDefaultAsync(cancellationToken) 
          ?? throw new ValidationException("User or refresh token not found.");

        UserDto userDto = _mapper.Map<UserDto>(existUser);

        ApiResponse<TokenDto> generatedToken = await _mediator.Send(new GenerateTokenQuery(userDto), cancellationToken);

        if (!generatedToken.IsSuccessful || generatedToken.Data is null)
            throw new BusinessException("Failed to create token");

        existUser.RefreshToken = generatedToken.Data.RefreshToken;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ApiResponse<TokenDto>
        {
            Data = generatedToken.Data,
            IsSuccessful = true,
            StatusCode = (int)HttpStatusCode.OK,
            TotalItemCount = 1
        };
    }
}