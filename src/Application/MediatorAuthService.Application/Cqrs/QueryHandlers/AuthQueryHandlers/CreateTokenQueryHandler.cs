using AutoMapper;
using MediatorAuthService.Application.Cqrs.Queries.AuthQueries;
using MediatorAuthService.Application.Dtos.AuthDtos;
using MediatorAuthService.Application.Dtos.UserDtos;
using MediatorAuthService.Application.Extensions;
using MediatorAuthService.Application.Wrappers;
using MediatorAuthService.Domain.Entities;
using MediatorAuthService.Infrastructure.UnitOfWork;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace MediatorAuthService.Application.Cqrs.QueryHandlers.AuthQueryHandlers;

public class CreateTokenQueryHandler : IRequestHandler<CreateTokenQuery, ApiResponse<TokenDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public CreateTokenQueryHandler(IUnitOfWork unitOfWork, IMediator mediator, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _mapper = mapper;
    }

    public async Task<ApiResponse<TokenDto>> Handle(CreateTokenQuery request, CancellationToken cancellationToken)
    {
        var existUser = await _unitOfWork.GetRepository<User>()
            .Where(x => x.Email.Equals(request.Email))
            .SingleOrDefaultAsync(cancellationToken);

        if (existUser is null)
            return new ApiResponse<TokenDto>
            {
                Errors = new List<string> { "User is not found." },
                IsSuccessful = false,
                StatusCode = (int)HttpStatusCode.NotFound
            };

        if (!existUser.IsActive)
            return new ApiResponse<TokenDto>
            {
                Errors = new List<string> { "The user is inactive in the system." },
                IsSuccessful = false,
                StatusCode = (int)HttpStatusCode.NotFound
            };

        if (!HashingManager.VerifyHashedValue(existUser.Password, request.Password))
            return new ApiResponse<TokenDto>
            {
                Errors = new List<string> { "User is not found." },
                IsSuccessful = false,
                StatusCode = (int)HttpStatusCode.NotFound
            };

        var userDto = _mapper.Map<UserDto>(existUser);

        var generatedToken = await _mediator.Send(new GenerateTokenQuery(userDto), cancellationToken);

        return new ApiResponse<TokenDto>
        {
            Data = generatedToken.Data,
            IsSuccessful = true,
            StatusCode = (int)HttpStatusCode.OK,
            TotalItemCount = 1
        };
    }
}