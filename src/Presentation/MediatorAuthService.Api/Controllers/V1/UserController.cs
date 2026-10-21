using Asp.Versioning;
using MediatorAuthService.Api.Authorization;
using MediatorAuthService.Api.Controllers.Base;
using MediatorAuthService.Api.Extensions;
using MediatorAuthService.Application.Common.Authorization;
using MediatorAuthService.Application.Cqrs.Commands.UserCommands;
using MediatorAuthService.Application.Cqrs.Queries.UserQueries;
using MediatorAuthService.Application.Dtos.ResponseDtos;
using MediatorAuthService.Application.Dtos.RoleDtos;
using MediatorAuthService.Application.Dtos.UserDtos;
using MediatorAuthService.Application.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
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
    [ApiVersion(1.0)]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ProducesResponseType(typeof(ApiResponse<NoData>), (int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<NoData>), (int)HttpStatusCode.Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<NoData>), (int)HttpStatusCode.BadRequest)]
    public class UserController(IMediator _mediator) : MediatorBaseController
    {
        /// <summary>
        /// Get One User
        /// </summary>
        /// <remarks>Users can read themselves; reading others requires Users.Read.</remarks>
        /// <param name="id"></param>
        /// <returns>Returns the user registered to ID</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<NoData>), (int)HttpStatusCode.NotFound)]
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
        [HasPermission(Permissions.Users.Read)]
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
        [HasPermission(Permissions.Users.Read)]
        [HttpGet("{email}")]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<NoData>), (int)HttpStatusCode.NotFound)]
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
        [HasPermission(Permissions.Users.Create)]
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ApiResponse<NoData>), (int)HttpStatusCode.Conflict)]
        public async Task<IActionResult> AddUser(CreateUserCommand user)
        {
            ApiResponse<UserDto> response = await _mediator.Send(user);

            return ActionResultInstance(response);
        }

        /// <summary>
        /// Update User
        /// </summary>
        /// <remarks>Users can update themselves; updating others (and activation status) requires Users.Update.</remarks>
        /// <param name="id"></param>
        /// <param name="user"></param>
        /// <returns>Updates the registered user's information.</returns>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<NoData>), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse<NoData>), (int)HttpStatusCode.Conflict)]
        public async Task<IActionResult> UpdateUser(Guid id, UpdateUserCommand user)
        {
            user.Id = id;

            ApiResponse<UserDto> response = await _mediator.Send(user);

            return ActionResultInstance(response);
        }

        /// <summary>
        /// Delete User
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Deletes the registered user from the system.</returns>
        [HasPermission(Permissions.Users.Delete)]
        [HttpDelete("{id:guid}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ApiResponse<NoData>), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            ApiResponse<NoData> response = await _mediator.Send(new DeleteUserCommand(id));

            return ActionResultInstance(response);
        }

        /// <summary>
        /// Change Password
        /// </summary>
        /// <remarks>Ends all sessions of the user; sign in again with the new password.</remarks>
        /// <param name="request"></param>
        /// <returns>Updates the registered user's password.</returns>
        [EnableRateLimiting(RateLimitingExtension.AuthPolicy)]
        [HttpPost("change-password")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> ChangePassword(ChangePasswordUserCommand request)
        {
            ApiResponse<NoData> response = await _mediator.Send(request);

            return ActionResultInstance(response);
        }

        /// <summary>
        /// Get User Roles
        /// </summary>
        /// <remarks>Users can read their own roles; reading the roles of others requires Users.Read.</remarks>
        /// <param name="id"></param>
        /// <returns>Roles assigned to the user.</returns>
        [HttpGet("{id:guid}/roles")]
        [ProducesResponseType(typeof(ApiResponse<List<RoleSummaryDto>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<NoData>), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetUserRoles(Guid id)
        {
            ApiResponse<List<RoleSummaryDto>> response = await _mediator.Send(new GetUserRolesQuery(id));

            return ActionResultInstance(response);
        }

        /// <summary>
        /// Set User Roles
        /// </summary>
        /// <remarks>
        /// Replaces the roles of the user. You can only add or remove roles whose permissions you hold,
        /// only a SuperAdmin can assign the SuperAdmin role, and the last SuperAdmin cannot lose it.
        /// </remarks>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns>The roles of the user after the change.</returns>
        [HasPermission(Permissions.Users.AssignRoles)]
        [HttpPut("{id:guid}/roles")]
        [ProducesResponseType(typeof(ApiResponse<List<RoleSummaryDto>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<NoData>), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> SetUserRoles(Guid id, SetUserRolesCommand request)
        {
            request.UserId = id;

            ApiResponse<List<RoleSummaryDto>> response = await _mediator.Send(request);

            return ActionResultInstance(response);
        }
    }
}
