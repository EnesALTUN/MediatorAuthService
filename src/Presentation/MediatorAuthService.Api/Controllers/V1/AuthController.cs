using MediatorAuthService.Api.Controllers.Base;
using MediatorAuthService.Application.Cqrs.Commands.AuthCommands;
using MediatorAuthService.Application.Cqrs.Queries.AuthQueries;
using MediatorAuthService.Application.Dtos.AuthDtos;
using MediatorAuthService.Application.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediatorAuthService.Api.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AuthController : MediatorBaseController
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("login")]
        public async Task<IActionResult> CreateToken(CreateTokenQuery request)
        {
            ApiResponse<TokenDto> response = await _mediator.Send(request);

            return ActionResultInstance(response);
        }

        [Authorize]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> CreateTokenByRefreshToken(CreateTokenByRefreshTokenCommand request)
        {
            ApiResponse<TokenDto> response = await _mediator.Send(request);

            return ActionResultInstance(response);
        }
    }
}
