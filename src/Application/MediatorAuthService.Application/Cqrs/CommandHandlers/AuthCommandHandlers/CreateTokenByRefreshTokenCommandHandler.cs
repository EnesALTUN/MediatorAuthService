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

public class CreateTokenByRefreshTokenCommandHandler : IRequestHandler<CreateTokenByRefreshTokenCommand, ApiResponse<TokenDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CreateTokenByRefreshTokenCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IMediator mediator, IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _mediator = mediator;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ApiResponse<TokenDto>> Handle(CreateTokenByRefreshTokenCommand request, CancellationToken cancellationToken)
    {
        Guid userId = _httpContextAccessor.HttpContext!.User.Id();

        User? existUser = await _unitOfWork.GetRepository<User>()
            .Where(user => user.Id.Equals(userId) && user.RefreshToken.Equals(request.RefreshToken) && user.IsActive)
            .SingleOrDefaultAsync(cancellationToken);

        if (existUser is null)
            throw new ValidationException("User or refresh token not found.");

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