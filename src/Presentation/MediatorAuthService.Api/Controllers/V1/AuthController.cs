using MediatorAuthService.Api.Controllers.Base;
using MediatorAuthService.Application.Cqrs.Commands.AuthCommands;
using MediatorAuthService.Application.Cqrs.Queries.AuthQueries;
using MediatorAuthService.Application.Dtos.AuthDtos;
using MediatorAuthService.Application.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace MediatorAuthService.Api.Controllers.V1
{
    /// <summary>
    /// Authorization - Authentication
    /// </summary>
    /// <remarks></remarks>
    /// <param name="_mediator"></param>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.BadRequest)]
    public class AuthController(IMediator _mediator) : MediatorBaseController
    {

        /// <summary>
        /// Login
        /// </summary>
        /// <param name="request"></param>
        /// <returns>JWT Access Token and Refresh Token</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<TokenDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateToken(CreateTokenQuery request)
        {
            ApiResponse<TokenDto> response = await _mediator.Send(request);

            return ActionResultInstance(response);
        }

        /// <summary>
        /// Create token used by refresh token
        /// </summary>
        /// <param name="request"></param>
        /// <returns>JWT Access Token and Refresh Token</returns>
        [Authorize]
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(ApiResponse<TokenDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> CreateTokenByRefreshToken(CreateTokenByRefreshTokenCommand request)
        {
            ApiResponse<TokenDto> response = await _mediator.Send(request);

            return ActionResultInstance(response);
        }
    }
}
