using Asp.Versioning;
using MediatorAuthService.Api.Authorization;
using MediatorAuthService.Api.Controllers.Base;
using MediatorAuthService.Application.Common.Authorization;
using MediatorAuthService.Application.Cqrs.Queries.RoleQueries;
using MediatorAuthService.Application.Dtos.ResponseDtos;
using MediatorAuthService.Application.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace MediatorAuthService.Api.Controllers.V1
{
    /// <summary>
    /// Permission Catalog
    /// </summary>
    /// <param name="_mediator"></param>
    [ApiController]
    [ApiVersion(1.0)]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ProducesResponseType(typeof(ApiResponse<NoData>), (int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<NoData>), (int)HttpStatusCode.Forbidden)]
    public class PermissionController(IMediator _mediator) : MediatorBaseController
    {
        /// <summary>
        /// Get All Permissions
        /// </summary>
        /// <returns>Every permission that can be assigned to a role, grouped by area.</returns>
        [HasPermission(Permissions.Roles.Read)]
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<PermissionDefinition>>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetPermissions()
        {
            ApiResponse<List<PermissionDefinition>> response = await _mediator.Send(new GetPermissionsQuery());

            return ActionResultInstance(response);
        }
    }
}
