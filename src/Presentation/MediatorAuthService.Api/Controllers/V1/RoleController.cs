using Asp.Versioning;
using MediatorAuthService.Api.Authorization;
using MediatorAuthService.Api.Controllers.Base;
using MediatorAuthService.Application.Common.Authorization;
using MediatorAuthService.Application.Cqrs.Commands.RoleCommands;
using MediatorAuthService.Application.Cqrs.Queries.RoleQueries;
using MediatorAuthService.Application.Dtos.ResponseDtos;
using MediatorAuthService.Application.Dtos.RoleDtos;
using MediatorAuthService.Application.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace MediatorAuthService.Api.Controllers.V1
{
    /// <summary>
    /// Role Management
    /// </summary>
    /// <remarks>
    /// Roles group permissions from the permission catalog. Changes apply to all users of a role immediately.
    /// You can only add or remove permissions you hold yourself. System roles (SuperAdmin) cannot be changed.
    /// </remarks>
    /// <param name="_mediator"></param>
    [Authorize]
    [ApiController]
    [ApiVersion(1.0)]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ProducesResponseType(typeof(ApiResponse<NoData>), (int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<NoData>), (int)HttpStatusCode.Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<NoData>), (int)HttpStatusCode.BadRequest)]
    public class RoleController(IMediator _mediator) : MediatorBaseController
    {
        /// <summary>
        /// Get All Roles
        /// </summary>
        /// <returns>Roles with their permissions and user counts.</returns>
        [HasPermission(Permissions.Roles.Read)]
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<RoleDto>>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetRoles()
        {
            ApiResponse<List<RoleDto>> response = await _mediator.Send(new GetRolesQuery());

            return ActionResultInstance(response);
        }

        /// <summary>
        /// Get One Role
        /// </summary>
        /// <param name="id"></param>
        /// <returns>The role with its permissions.</returns>
        [HasPermission(Permissions.Roles.Read)]
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<RoleDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<NoData>), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetRoleById(Guid id)
        {
            ApiResponse<RoleDto> response = await _mediator.Send(new GetRoleByIdQuery(id));

            return ActionResultInstance(response);
        }

        /// <summary>
        /// Create Role
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The created role.</returns>
        [HasPermission(Permissions.Roles.Create)]
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<RoleDto>), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ApiResponse<NoData>), (int)HttpStatusCode.Conflict)]
        public async Task<IActionResult> CreateRole(CreateRoleCommand request)
        {
            ApiResponse<RoleDto> response = await _mediator.Send(request);

            return ActionResultInstance(response);
        }

        /// <summary>
        /// Update Role
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request">Name, description and the complete permission list of the role.</param>
        /// <returns>The updated role.</returns>
        [HasPermission(Permissions.Roles.Update)]
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<RoleDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<NoData>), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse<NoData>), (int)HttpStatusCode.Conflict)]
        public async Task<IActionResult> UpdateRole(Guid id, UpdateRoleCommand request)
        {
            request.Id = id;

            ApiResponse<RoleDto> response = await _mediator.Send(request);

            return ActionResultInstance(response);
        }

        /// <summary>
        /// Delete Role
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Deletes the role and removes it from every user.</returns>
        [HasPermission(Permissions.Roles.Delete)]
        [HttpDelete("{id:guid}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ApiResponse<NoData>), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteRole(Guid id)
        {
            ApiResponse<NoData> response = await _mediator.Send(new DeleteRoleCommand(id));

            return ActionResultInstance(response);
        }
    }
}
