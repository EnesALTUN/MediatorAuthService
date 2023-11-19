using MediatorAuthService.Api.Controllers.Base;
using MediatorAuthService.Application.Cqrs.Commands.UserCommands;
using MediatorAuthService.Application.Cqrs.Queries.UserQueries;
using MediatorAuthService.Application.Dtos.ResponseDtos;
using MediatorAuthService.Application.Dtos.UserDtos;
using MediatorAuthService.Application.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace MediatorAuthService.Api.Controllers.V1
{
    /// <summary>
    /// User Operations
    /// </summary>
    /// <remarks></remarks>
    /// <param name="_mediator"></param>
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ProducesResponseType(typeof(void), (int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.BadRequest)]
    public class UserController(IMediator _mediator) : MediatorBaseController
    {

        /// <summary>
        /// Get One User
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Returns the user registered to ID</returns>
        [HttpGet("{id:Guid}")]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetById(Guid id)
        {
            ApiResponse<UserDto> response = await _mediator.Send(new GetUserByIdQuery(id));

            return ActionResultInstance(response);
        }

        /// <summary>
        /// Get All Users
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Returns all users that match the given filter.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<UserDto>>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAllUser([FromQuery] GetUserAllQuery request)
        {
            ApiResponse<List<UserDto>> response = await _mediator.Send(request);

            return ActionResultInstance(response);
        }

        /// <summary>
        /// Get One User By Email
        /// </summary>
        /// <param name="email"></param>
        /// <returns>Returns user information registered to the e-mail address.</returns>
        [HttpGet("{email}")]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            ApiResponse<UserDto> response = await _mediator.Send(new GetUserByEmailQuery(email));

            return ActionResultInstance(response);
        }

        /// <summary>
        /// Create User
        /// </summary>
        /// <param name="user"></param>
        /// <returns>Creates a user and returns relevant information.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> AddUser(CreateUserCommand user)
        {
            ApiResponse<UserDto> response = await _mediator.Send(user);

            return ActionResultInstance(response);
        }

        /// <summary>
        /// Update User
        /// </summary>
        /// <param name="user"></param>
        /// <returns>Updates the registered user's information.</returns>
        [HttpPut]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateUser(UpdateUserCommand user)
        {
            ApiResponse<UserDto> response = await _mediator.Send(user);

            return ActionResultInstance(response);
        }

        /// <summary>
        /// Delete User
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Deletes the registered user from the system.</returns>
        [HttpDelete("{id:Guid}")]
        [ProducesResponseType(typeof(ApiResponse<NoDataDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            ApiResponse<NoDataDto> response = await _mediator.Send(new DeleteUserCommand(id));

            return ActionResultInstance(response);
        }

        /// <summary>
        /// Change Password
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Updates the registered user's password.</returns>
        [HttpPost("change-password")]
        [ProducesResponseType(typeof(ApiResponse<NoDataDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> ChangePassword(ChangePasswordUserCommand request)
        {
            ApiResponse<NoDataDto> response = await _mediator.Send(request);

            return ActionResultInstance(response);
        }
    }
}