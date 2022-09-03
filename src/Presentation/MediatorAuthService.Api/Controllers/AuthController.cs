using MediatorAuthService.Api.Controllers.Base;
using MediatorAuthService.Application.Cqrs.Commands.AuthCommands;
using MediatorAuthService.Application.Cqrs.Queries.AuthQueries;
using MediatorAuthService.Application.Dtos.AuthDtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediatorAuthService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
            var response = await _mediator.Send(request);

            return ActionResultInstance<TokenDto>(response);
        }

        [Authorize]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> CreateTokenByRefreshToken(CreateTokenByRefreshTokenCommand request)
        {
            var response = await _mediator.Send(request);

            return ActionResultInstance<TokenDto>(response);
        }
    }
}
