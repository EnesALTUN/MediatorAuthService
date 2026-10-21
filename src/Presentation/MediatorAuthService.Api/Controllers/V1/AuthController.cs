using Asp.Versioning;
using MediatorAuthService.Api.Controllers.Base;
using MediatorAuthService.Api.Extensions;
using MediatorAuthService.Application.Cqrs.Commands.AuthCommands;
using MediatorAuthService.Application.Cqrs.Queries.AuthQueries;
using MediatorAuthService.Application.Dtos.AuthDtos;
using MediatorAuthService.Application.Dtos.ResponseDtos;
using MediatorAuthService.Application.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Net;

namespace MediatorAuthService.Api.Controllers.V1
{
    /// <summary>
    /// Authorization - Authentication
    /// </summary>
    /// <remarks></remarks>
    /// <param name="_mediator"></param>
    [ApiController]
    [ApiVersion(1.0)]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ProducesResponseType(typeof(ApiResponse<NoData>), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<NoData>), (int)HttpStatusCode.TooManyRequests)]
    public class AuthController(IMediator _mediator) : MediatorBaseController
    {
        /// <summary>
        /// Login
        /// </summary>
        /// <param name="request"></param>
        /// <returns>JWT Access Token and Refresh Token</returns>
        [AllowAnonymous]
        [EnableRateLimiting(RateLimitingExtension.AuthPolicy)]
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<TokenDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<NoData>), (int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> Login(LoginCommand request)
        {
            ApiResponse<TokenDto> response = await _mediator.Send(request);

            return ActionResultInstance(response);
        }

        /// <summary>
        /// Create token used by refresh token
        /// </summary>
        /// <remarks>
        /// No access token is required. The presented refresh token is revoked and a new pair is returned (rotation).
        /// Re-using a revoked refresh token ends every session of the user.
        /// </remarks>
        /// <param name="request"></param>
        /// <returns>JWT Access Token and Refresh Token</returns>
        [AllowAnonymous]
        [EnableRateLimiting(RateLimitingExtension.AuthPolicy)]
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(ApiResponse<TokenDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<NoData>), (int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> CreateTokenByRefreshToken(CreateTokenByRefreshTokenCommand request)
        {
            ApiResponse<TokenDto> response = await _mediator.Send(request);

            return ActionResultInstance(response);
        }

        /// <summary>
        /// Logout
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Revokes the given refresh token.</returns>
        [AllowAnonymous]
        [EnableRateLimiting(RateLimitingExtension.AuthPolicy)]
        [HttpPost("logout")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> Logout(LogoutCommand request)
        {
            ApiResponse<NoData> response = await _mediator.Send(request);

            return ActionResultInstance(response);
        }

        /// <summary>
        /// Current User
        /// </summary>
        /// <returns>The signed-in user with roles and effective permissions, for deciding which screens and actions to show.</returns>
        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(typeof(ApiResponse<CurrentUserDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<NoData>), (int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> Me()
        {
            ApiResponse<CurrentUserDto> response = await _mediator.Send(new GetCurrentUserQuery());

            return ActionResultInstance(response);
        }
    }
}
